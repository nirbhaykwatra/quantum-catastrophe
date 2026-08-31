using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;

// moves platforms using Rigidbodies
public class MovingPlatform : MonoBehaviour
{
    private enum PhysicsMode
    {
        Physics3D,
        Physics2D
    }

    private enum MovementMode
    {
        Loop,             // cycles through all points forever (original behaviour)
        Once,             // moves through the points a single time and stops at the last one
        PlayerControlled  // only advances while the player is standing on it when _playerOffBehavior is ReturnToFirstPoint; see _playerOffBehavior for what happens once the player steps off
    }

    // only used by PlayerControlled - what the platform does once the player steps off before it reaches point 0
    private enum PlayerOffBehavior
    {
        ReturnToFirstPoint,  // retreats back through every point already visited until it reaches point 0
        ContinueToLastPoint  // ignores the player being gone and keeps advancing toward the last point
    }

    // points to loop through
    [SerializeField] private bool _Active = true;
    [SerializeField] private Vector3[] _points = new Vector3[] { -Vector3.right, Vector3.right };
    [SerializeField] private float _speed = 2f;
    // moves to next point within distance
    [SerializeField] private float _pointReachedDistance = 0.05f;
    // slows movement within distance
    [SerializeField] private float _easingDistance = 1f;
    [SerializeField] private PhysicsMode _physicsMode;
    [SerializeField] private MovementMode _movementMode = MovementMode.Loop;
    // only used by Loop / Once - PlayerControlled always waits for the player regardless of this flag
    [SerializeField] private bool _activateOnStep = false;
    // only used by PlayerControlled - see PlayerOffBehavior above
    [SerializeField] private PlayerOffBehavior _playerOffBehavior = PlayerOffBehavior.ReturnToFirstPoint;
    [SerializeField] private GameObject m_entanglementSelector;

    public Vector3 NextPoint => _startPosition + _points[_pointIndex % _points.Length];
    public Vector3 PreviousPoint => _startPosition + _points[(_pointIndex + _points.Length - 1) % _points.Length];

    private Vector3 _startPosition;
    private int _pointIndex;
    private bool _playerOnPlatform = false;
    private Rigidbody2D _rb2D;
    private Rigidbody _rb3D;
    private BoxCollider2D _collider2D;
    private bool _started = false;
    private void OnValidate()
    {
        _collider2D = GetComponent<BoxCollider2D>();
    }

    private void Awake()
    {
        _startPosition = transform.position;
        // get both 2D and 3D rigidbodies and then set flag for which one was found
        switch (_physicsMode)
        {
            case PhysicsMode.Physics3D:
                _rb3D = GetComponent<Rigidbody>();
                if (_rb3D == null) _rb3D = gameObject.AddComponent<Rigidbody>();
                _rb3D.isKinematic = true;
                _rb3D.interpolation = RigidbodyInterpolation.Interpolate;
                break;
            case PhysicsMode.Physics2D:
                _rb2D = GetComponent<Rigidbody2D>();
                if (_rb2D == null) _rb2D = gameObject.AddComponent<Rigidbody2D>();
#if UNITY_6000_0_OR_NEWER
                _rb2D.bodyType = RigidbodyType2D.Kinematic;
#else
                _rb2D.isKinematic = true;
#endif
                _rb2D.interpolation = RigidbodyInterpolation2D.Interpolate;
                break;
        }
    }

    private void FixedUpdate()
    {
        MovePlatform();
    }

    private void MovePlatform()
    {
        if (!_Active) return;

        // PlayerControlled ignores _activateOnStep entirely - whether it moves at all
        // is driven purely by whether the player is currently on the platform.
        if (_movementMode == MovementMode.PlayerControlled)
        {
            MovePlayerControlled();
            return;
        }

        if (_activateOnStep && !_started) return;

        switch (_movementMode)
        {
            case MovementMode.Loop:
                MoveLoop();
                break;
            case MovementMode.Once:
                MoveOnce();
                break;
        }
    }

    private void MoveLoop()
    {
        // checks if point is reached
        float distance = Vector3.Distance(transform.position, NextPoint);
        if (distance < _pointReachedDistance) _pointIndex++;

        ApplyVelocityTowards(NextPoint, PreviousPoint);
    }

    private void MoveOnce()
    {
        int targetIndex = Mathf.Min(_pointIndex, _points.Length - 1);
        Vector3 target = _startPosition + _points[targetIndex];
        float distance = Vector3.Distance(transform.position, target);

        // reached the final point - stay put for good
        if (targetIndex >= _points.Length - 1 && distance < _pointReachedDistance)
        {
            StopMoving();
            return;
        }

        if (distance < _pointReachedDistance) _pointIndex++;

        Vector3 previous = _startPosition + _points[Mathf.Max(targetIndex - 1, 0)];
        ApplyVelocityTowards(target, previous);
    }

    private void MovePlayerControlled()
    {
        // ContinueToLastPoint keeps heading toward the last point even once the player
        // steps off; ReturnToFirstPoint (default) only advances while the player is aboard
        bool wantsForward = _playerOnPlatform || _playerOffBehavior == PlayerOffBehavior.ContinueToLastPoint;

        bool movingForward = wantsForward && _pointIndex < _points.Length - 1;
        bool movingBackward = !wantsForward && _pointIndex > 0;

        // nothing left to do given the current direction and position
        if (!movingForward && !movingBackward)
        {
            StopMoving();
            return;
        }

        int targetIndex = movingForward ? _pointIndex + 1 : _pointIndex - 1;
        Vector3 target = _startPosition + _points[targetIndex];
        Vector3 previous = _startPosition + _points[_pointIndex];

        float distance = Vector3.Distance(transform.position, target);
        if (distance < _pointReachedDistance)
        {
            // arrived at the next waypoint along the current direction of travel;
            // re-evaluate next FixedUpdate in case the player's state changed mid-step
            _pointIndex = targetIndex;
            return;
        }

        ApplyVelocityTowards(target, previous);
    }

    // shared easing + velocity calculation, used by all movement modes
    private void ApplyVelocityTowards(Vector3 target, Vector3 previous)
    {
        Vector3 dir = (target - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, target);
        float distanceToPrevious = Vector3.Distance(transform.position, previous);
        float previousEasing = distanceToPrevious / _easingDistance;
        float nextEasing = distanceToTarget / _easingDistance;
        float easing = Mathf.Min(previousEasing, nextEasing);
        easing = Mathf.Clamp(easing, 0.01f, 1f);
        ApplyVelocity(dir * _speed * easing);
    }

    private void ApplyVelocity(Vector3 velocity)
    {
        switch (_physicsMode)
        {
            case PhysicsMode.Physics3D:
                _rb3D.MovePosition(transform.position + velocity * Time.fixedDeltaTime);
                break;
            case PhysicsMode.Physics2D:
#if UNITY_6000_0_OR_NEWER
                _rb2D.linearVelocity = velocity;
#else
                _rb2D.velocity = velocity;
#endif
                break;
        }
    }

    private void StopMoving()
    {
        ApplyVelocity(Vector3.zero);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.GetComponent<PlayerController>())
        {
            _started = true;
            _playerOnPlatform = true;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.GetComponent<PlayerController>())
        {
            _playerOnPlatform = false;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.GetComponent<PlayerController>())
        {
            _started = true;
            _playerOnPlatform = true;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.GetComponent<PlayerController>())
        {
            _playerOnPlatform = false;
        }
    }

    public void Activate()
    {
        _started = true;
        _Active = true;
    }
    public void Deactivate()
    {
        _started = false;
        _Active = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        Gizmos.color = Color.green;
        Vector3 origin = Application.isPlaying ? _startPosition : transform.position;

        for (int i = 0; i < _points.Length; i++)
        {
            Vector3 point = origin + _points[i];
            Gizmos.DrawWireSphere(point, 0.1f);
            if (_collider2D != null) Gizmos.DrawWireCube(point, _collider2D.size);
        }

        // Loop draws a closing line back to point 0; Once/PlayerControlled are a path, not a cycle
        int segmentCount = _movementMode == MovementMode.Loop ? _points.Length : _points.Length - 1;
        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 point = origin + _points[i];
            Vector3 nextPoint = origin + _points[(i + 1) % _points.Length];
            Gizmos.DrawLine(point, nextPoint);
        }
    }
}