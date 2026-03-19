using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;

// moves platforms using Rigidbodies
public class MovingPlatform : MonoBehaviour, IEntangleable
{
    private enum PhysicsMode
    {
        Physics3D,
        Physics2D
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
    [SerializeField] private bool _activateOnStep = false;
    [SerializeField] private GameObject m_entanglementSelector;

    public Vector3 NextPoint => _startPosition + _points[_pointIndex % _points.Length];
    public Vector3 PreviousPoint => _startPosition + _points[(_pointIndex + _points.Length - 1) % _points.Length];

    private Vector3 _startPosition;
    private int _pointIndex;
    private Rigidbody2D _rb2D;
    private Rigidbody _rb3D;
    private BoxCollider2D _collider2D;
    private bool _started = false;
    private GlobalEventBus m_globalEventBus;
    
    private EventBinding<OnModeChange> m_onModeChange;
    private bool m_entangled = false;

    private void OnValidate()
    {
        _collider2D = GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        m_globalEventBus = ServiceLocator.Global.Get<EventBusRegistry>().Get<GlobalEventBus>();
        m_onModeChange = new EventBinding<OnModeChange>(ActivateEntanglementSelector);
        m_globalEventBus.Register(m_onModeChange);
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
        if (m_entangled)
        {
            MovePlatformEntangled();
        }
        else
        {
            MovePlatform();
        }
    }

    private void MovePlatform()
    {
        if (!_Active) return;
        if (_activateOnStep && !_started) return;
        // checks if point is reached
        float distance = Vector3.Distance(transform.position, NextPoint);
        if (distance < _pointReachedDistance) _pointIndex++;

        // calculates movement direction/speed/easing
        Vector3 dir = (NextPoint - transform.position).normalized;
        float distanceToPrevious = Vector3.Distance(transform.position, PreviousPoint);
        float previousEasing = distanceToPrevious / _easingDistance;
        float nextEasing = distance / _easingDistance;
        float easing = Mathf.Min(previousEasing, nextEasing);
        easing = Mathf.Clamp(easing, 0.01f, 1f);
        Vector3 velocity = dir * _speed * easing;

        // move according to physics mode
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

    private void MovePlatformEntangled()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.GetComponent<PlayerController>())
        {
            _started = true;
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
            Vector3 nextPoint = origin + _points[(i + 1) % _points.Length];
            Gizmos.DrawWireSphere(point, 0.1f);
            Gizmos.DrawWireCube(point, _collider2D.size);
            Gizmos.DrawLine(point, nextPoint);
        }
    }

    private void ActivateEntanglementSelector(OnModeChange @event)
    {
        m_entanglementSelector.SetActive(@event.Mode == PlayerMode.Entangle);
    }

    public void OnEntanglementSelected()
    {
        SpriteRenderer spriteRenderer = m_entanglementSelector.GetComponent<SpriteRenderer>();
        spriteRenderer.color = Color.green;
    }

    public void OnEntanglementDeselected()
    {
        SpriteRenderer spriteRenderer = m_entanglementSelector.GetComponent<SpriteRenderer>();
        spriteRenderer.color = Color.white;
    }

    public void OnEntangle(IEntangleable other)
    {
        m_entangled = true;
    }

    public void OnEntanglementBroken()
    {
        m_entangled = false;
    }
}