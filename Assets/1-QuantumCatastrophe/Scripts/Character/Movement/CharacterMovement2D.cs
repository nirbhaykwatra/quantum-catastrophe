using System.Collections;
using System.Collections.Generic;
using QC.Character;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterMovement2D : CharacterMovementBase
{
    [field: Header("Top Down")]
    [field: SerializeField] protected bool TopDownMovement = false;

    [field: Header("Components")]
    [field: SerializeField] protected Rigidbody2D Rigidbody;
    [field: SerializeField] protected CapsuleCollider2D CapsuleCollider;
    
    [field: Header("Wall Jump")]
    [field: SerializeField] protected float WallJumpVelocity = 1.25f;
    [field: SerializeField] protected float WallJumpHeight = 2f;
    [field: SerializeField] protected float WallJumpHorizontalSpeed = 6f;
    [field: SerializeField] protected float WallJumpControlLockDuration = 0.15f;

#if UNITY_6000_0_OR_NEWER
    public override Vector3 Velocity { get => Rigidbody.linearVelocity; protected set => Rigidbody.linearVelocity = value; }
#else
    public override Vector3 Velocity { get => Rigidbody.velocity; protected set => Rigidbody.velocity = value; }
#endif
    public bool CanDashJump { get; set; }
    protected Vector3 GroundCheckStart => transform.position + transform.up * GroundCheckOffset;
    protected Vector3 WallCheckStart => transform.position 
                                        + transform.up * WallCheckOffset 
                                        + (Vector3)(LookDirection.normalized * (Radius * 0.5f));
    
    [ReadOnly]
    public bool CanWallJump;
    [ReadOnly]
    [ShowInInspector]
    private int m_jumpCount;
    private bool m_wasGrounded;
    
    protected CharacterAbilities m_abilities;

    protected virtual void OnValidate()
    {
        if (Rigidbody == null) Rigidbody = GetComponent<Rigidbody2D>();
        Rigidbody.gravityScale = 0f;
        Rigidbody.freezeRotation = true;

        if (CapsuleCollider == null) CapsuleCollider = GetComponentInChildren<CapsuleCollider2D>();
        if (CapsuleCollider != null)
        {
            CapsuleCollider.size = new Vector2(Radius, Height);
            CapsuleCollider.offset = new Vector2(0f, Height * 0.5f);
        }
        
        m_abilities = GetComponent<CharacterAbilities>();
    }
    
    protected virtual void Awake()
    {
        if (CapsuleCollider != null)
        {
            CapsuleCollider.sharedMaterial = new PhysicsMaterial2D("NoFriction") { friction = 0f, bounciness = 0f };
            CapsuleCollider.size = new Vector2(Radius, Height);
            CapsuleCollider.offset = new Vector2(0f, Height * 0.5f);
        }

        LookDirection = Vector3.right;
        
        m_abilities = GetComponent<CharacterAbilities>();
    }

    // receives movement input and clamps it to prevent over-acceleration
    public override void SetMoveInput(Vector3 input)
    {
        if (!CanMove && !m_abilities.IsDashing)
        {
            MoveInput = Vector3.zero;
            return;
        }

        input = Vector3.ClampMagnitude(input, 1f);
        // set input to 0 if small incoming value
        HasMoveInput = input.magnitude > 0.1f;
        input = HasMoveInput ? input : Vector3.zero;
        MoveInput = input;
        // finds movement input as local direction rather than world
        LocalMoveInput = transform.InverseTransformDirection(MoveInput);
    }

    // sets character look direction, flattening y-value
    public override void SetLookDirection(Vector3 direction)
    {
        if (!CanTurn || direction.magnitude < 0.1f) return;
        LookDirection = new Vector3(direction.x, 0f, direction.z).normalized;
    }

    public override void SetLookPosition(Vector3 position)
    {
        Vector3 direction = Vector3.ClampMagnitude(position - transform.position, 1f);
        SetLookDirection(direction);
    }

    // attempts a jump, will fail if not grounded
    public override void TryJump()
    {
        if (CanWallJump && !IsGrounded) TryWallJump();
        if (IsGrounded)
        {
            if (!CanMove || !CanCoyoteJump) return;
            m_jumpCount = 0;
            Jump();
        }
        else
        {
            if (!m_abilities.EnableDoubleJump) return;
            if (!CanMove || m_jumpCount > 1) return;
            Velocity = Vector3.zero;
            Jump();
        }
    }
    
    public void ResetJumpCount() => m_jumpCount = 0;
    public void ResetMidAirJumpCount() => m_jumpCount = 1;

    public void TryWallJump()
    {
        if (!m_abilities.EnableWallJump) return;
        if (!CanMove || !CanWallJump) return;
        WallJump();
    }

    private void WallJump()
    {
        float verticalVelocity = Mathf.Sqrt(2f * -Gravity * WallJumpHeight);
        float horizontalVelocity = -LookDirection.x * WallJumpHorizontalSpeed;
        Velocity = new Vector3(horizontalVelocity, verticalVelocity, Velocity.z);

        LookDirection = new Vector3(-LookDirection.x, 0f, 0f);

        m_abilities.OnWallJump();
        StartCoroutine(WallJumpControlLock());
    }

    private IEnumerator WallJumpControlLock()
    {
        CanMove = false;
        yield return new WaitForSeconds(WallJumpControlLockDuration);
        CanMove = true;
    }

    public override void Jump()
    {
        // calculate jump velocity from jump height and gravity
        float jumpVelocity = CanDashJump ? Mathf.Sqrt(2f * -Gravity * JumpHeight * m_abilities.DashJumpBoostMultiplier) : Mathf.Sqrt(2f * -Gravity * JumpHeight);
        // override current y velocity but maintain x/z velocity
        Velocity = new Vector3(Velocity.x, jumpVelocity, Velocity.z);
        m_jumpCount++;
    }

    protected virtual void FixedUpdate()
    {
        // check for the ground
        bool wasGrounded = IsGrounded;
        IsGrounded = CheckGrounded();
        if (IsGrounded && !wasGrounded) ResetJumpCount(); // only on the landing edge
        CanWallJump = CheckWallContact();

        // sends correct forward/right inputs to GetMovementAcceleration and applies result to rigidbody
        Vector3 input = MoveInput;
        Vector3 forward = Vector3.right * input.x;

        // calculates desirection movement velocity
        Vector3 targetVelocity = forward * (Speed * MoveSpeedMultiplier);
        if (!CanMove) targetVelocity = Vector3.zero;
        // adds velocity of surface under character, if character is stationary
        targetVelocity += SurfaceVelocity * (1f - Mathf.Abs(MoveInput.magnitude));
        // calculates acceleration required to reach desired velocity and applies air control if not grounded
        Vector3 velocityDiff = targetVelocity - Velocity;
        if (!TopDownMovement) velocityDiff.y = 0f;
        float control = IsGrounded ? 1f : AirControl;
        Vector3 acceleration = velocityDiff * (Acceleration * control);
        // zeros acceleration if airborne and not trying to move (allows for nice jumping arcs)
        if (!IsGrounded && !HasMoveInput)
        {
            acceleration = velocityDiff * Acceleration;
        }
        // add gravity
        acceleration += GroundNormal * Gravity;

        Rigidbody.AddForce(acceleration);

        if (CanWallJump && !IsGrounded)
        {
            Rigidbody.linearVelocity = new Vector2(Velocity.x, Mathf.Max(Velocity.y, -2));
        }

        // rotates character towards movement direction
        if (ControlRotation && (IsGrounded || AirTurning))
        {
            transform.rotation = Quaternion.LookRotation(LookDirection);
        }
        // fix capsule collider rotation
        CapsuleCollider.transform.rotation = Quaternion.identity;
    }

    protected virtual bool CheckGrounded()
    {
        // ignore ground checks if top-down
        if (TopDownMovement) return true;

        // configure layer mask for 2D raycast
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(GroundMask);
        // raycast to find ground
        RaycastHit2D[] hits = new RaycastHit2D[4];
        RaycastHit2D groundHit = new RaycastHit2D();
        Physics2D.Raycast(GroundCheckStart, -transform.up, filter, hits, GroundCheckDistance);
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit2D hit = hits[i];
            if(hit.collider != null && hit.collider != CapsuleCollider)
            {
                groundHit = hit;
                continue;
            }
        }

        // set default ground surface normal and SurfaceVelocity
        GroundNormal = Vector3.up;
        SurfaceVelocity = Vector3.zero;

        // if ground wasn't hit, character is not grounded
        if (groundHit.collider == null) return false;

        // gets velocity of surface underneath character if applicable
#if UNITY_6000_0_OR_NEWER
        if (groundHit.rigidbody != null) SurfaceVelocity = groundHit.rigidbody.linearVelocity;
#else
        if (groundHit.rigidbody != null) SurfaceVelocity = groundHit.rigidbody.velocity;
#endif

        // test angle between character up and ground, angles above _maxSlopeAngle are invalid
        bool angleValid = Vector3.Angle(transform.up, groundHit.normal) < MaxSlopeAngle;
        if (angleValid)
        {
            // record last time character was grounded and set correct floor normal direction
            LastGroundedTime = Time.timeSinceLevelLoad;
            GroundNormal = groundHit.normal;
            LastGroundedPosition = transform.position;
            SurfaceObject = groundHit.collider.gameObject;
            if (ParentToSurface) transform.SetParent(SurfaceObject.transform);
            return true;
        }

        SurfaceObject = null;
        if (ParentToSurface) transform.SetParent(null);
        return false;
    }

    // check for landing on the ground
    private void OnCollisionEnter2D(Collision2D collision)
    {
        float landingCollisionMaxDistance = 0.25f;
        Vector3 point = collision.contacts[0].point;
        if (Mathf.Abs(collision.relativeVelocity.y) < MinGroundedVelocity) return;
        if (Vector3.Distance(point, transform.position) < landingCollisionMaxDistance)
        {
            OnGrounded.Invoke(collision.gameObject);
            OnGroundedEvent();
        }
    }

    protected void OnGroundedEvent()
    {
        m_abilities.OnGrounded();
    }

    protected bool CheckWallContact()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(WallMask);
        RaycastHit2D[] hits = new RaycastHit2D[4];
        int hitCount = Physics2D.Raycast(WallCheckStart, LookDirection, filter, hits, WallCheckDistance);

        float closestDistance = float.MaxValue;
        bool foundWall = false;
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = hits[i];
            if (hit.collider == null || hit.collider == CapsuleCollider) continue;
            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                foundWall = true;
            }
        }

        return foundWall;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(GroundCheckStart, -transform.up * GroundCheckDistance);
        
        // Gizmos.color = CheckWallContact() ? Color.green : Color.red;
        // Gizmos.DrawRay(WallCheckStart, LookDirection * WallCheckDistance);
    }
}