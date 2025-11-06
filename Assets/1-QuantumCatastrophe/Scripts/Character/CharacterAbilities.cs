using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

public enum Abilities
{
    Dash,
    AirDash,
    WallJump,
    DoubleJump,
    EntanglementMode,
    TunnelingBarriers,
    Superposition
}

public class CharacterAbilities : MonoBehaviour
{
    // TODO: Abilities
    //  Platforming
    //      Dash
    //      Air Dash
    //      Wall Jump
    //      Double Jump
    //  Puzzle/Combat
    //      Entanglement mode
    //      Tunneling Barriers
    //      Superposition Something
    //      
    
    // TODO: Fix dash cooldowns and air dash grounded reset
    
    [field: Header("Abilities")]
    [field: SerializeField]
    public bool EnableDash { get; set; } = false;
    [field: SerializeField]
    public bool EnableAirDash { get; set; } = false;
    [field: SerializeField]
    public bool EnableWallJump { get; set; } = false;
    [field: SerializeField]
    public bool EnableDoubleJump { get; set; } = false;
    [field: SerializeField]
    public bool EnableEntanglementMode { get; set; } = false;
    [field: SerializeField]
    public bool EnableTunnelingBarriers { get; set; } = false;
    [field: SerializeField]
    public bool EnableSuperposition { get; set; } = false;
    
    [field: Header("Dashing")]
    [field: SerializeField]
    public float DashDuration { get; set; }
    [field: SerializeField]
    public float DashDistance { get; set; }
    [field: SerializeField]
    public float DashCooldown { get; set; }
    [field: SerializeField]
    public float DashCheckOffset { get; set; } = 1f;
    [field: SerializeField]
    public float DashCheckRadius { get; set; } = 0.25f;
    [field: SerializeField]
    public LayerMask GroundMask { get; set; }
    

    [Title("Read-Only Fields")] 
    [ShowInInspector] [ReadOnly] public bool IsDashing { get; private set; }
    [ShowInInspector] [ReadOnly] public bool CanDash { get; set; } = true;
    [ShowInInspector] [ReadOnly] public bool CanAirDash { get; set; }
    
    private Rigidbody2D m_rigidbody;
    private CharacterMovement2D m_movement;
    private float m_initialGravityScale;
    private float m_dashCooldownTimer;
    private Vector2 m_dashDestination;
    private float m_airboneTimer;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_movement = GetComponent<CharacterMovement2D>();
    }

    private void Start()
    {
        IsDashing = false;
        m_dashCooldownTimer = DashCooldown;
        m_initialGravityScale = m_rigidbody.gravityScale;
    }

    private void Update()
    {
        if (!CanDash && m_movement.IsGrounded)
        {
            m_dashCooldownTimer -= Time.deltaTime;
            if (m_dashCooldownTimer <= 0)
            {
                CanDash = true;
            }
        }

        if (!m_movement.IsGrounded)
        {
            m_airboneTimer += Time.deltaTime;
        }
        
    }

    public void TryDash()
    {
        if (!EnableDash) return;
        if (IsDashing || !CanDash || !m_movement.IsGrounded) return;
        StartCoroutine(OnDash());
        CanDash = false;
        m_dashCooldownTimer = DashCooldown;
    }

    public void TryAirDash()
    {
        if (!EnableAirDash) return;
        if (!CanDash || !CanAirDash) return;
        StartCoroutine(OnDash());
        CanAirDash = false;
        CanDash = false;
        m_dashCooldownTimer = DashCooldown;
    }

    public void OnGrounded()
    {
        Debug.Log("Airborne for " + m_airboneTimer + " seconds");
        m_airboneTimer = 0f;
    }

    public IEnumerator OnDash()
    {
        if (!EnableDash) yield break;
        if (!m_movement.IsGrounded && !EnableAirDash) yield break;
        if (!CanDash || IsDashing || !CanAirDash) yield break;
        float timer = 0f;
        float progress = 0f;
        m_rigidbody.gravityScale = 0f;
        m_movement.CanMove = false;
        m_movement.CanTurn = false;
        IsDashing = true;
        
        // find start/end positions
        Vector2 direction = m_movement.MoveInput.magnitude > 0.1f ? m_movement.MoveInput : transform.forward;
        Vector2 start = transform.position;
        Vector2 destination = start + (direction * DashDistance);
        
        RaycastHit2D hit = Physics2D.Linecast(start + Vector2.up * DashCheckOffset, destination + Vector2.up * DashCheckOffset, GroundMask);
        if (hit.collider != null)
        {
            destination = start + direction * (hit.distance - DashCheckRadius);
        }
        
        m_dashDestination = destination;

        float velocity = DashDistance / DashDuration;
        float duration = Vector2.Distance(start, destination) / velocity; 
        
        while (progress < 1f)
        {
            timer += Time.deltaTime;
            progress = timer / duration;
            
            Vector2 position = Vector2.Lerp(start, destination, progress);
            m_rigidbody.MovePosition(position);
            
            yield return null;
        }
        
        m_rigidbody.gravityScale = m_initialGravityScale;
        m_movement.CanMove = true;
        m_movement.CanTurn = true;
        IsDashing = false;
    }

    public void UnlockAbility(Abilities ability)
    {
        switch (ability)
        {
            case Abilities.Dash:
                EnableDash = true;
                break;
            case Abilities.AirDash:
                EnableAirDash = true;
                break;
            case Abilities.WallJump:
                EnableWallJump = true;
                break;
            case Abilities.DoubleJump:
                EnableDoubleJump = true;
                break;
            case Abilities.EntanglementMode:
                EnableEntanglementMode = true;
                break;
            case Abilities.TunnelingBarriers:
                EnableTunnelingBarriers = true;
                break;
            case Abilities.Superposition:
                EnableSuperposition = true;
                break;
            default:
                throw new System.ArgumentOutOfRangeException(nameof(ability), ability, null);
        }
    }

    public void LockAbility(Abilities ability)
    {
        switch (ability)
        {
            case Abilities.Dash:
                EnableDash = false;
                break;
            case Abilities.AirDash:
                EnableAirDash = false;
                break;
            case Abilities.WallJump:
                EnableWallJump = false;
                break;
            case Abilities.DoubleJump:
                EnableDoubleJump = false;
                break;
            case Abilities.EntanglementMode:
                EnableEntanglementMode = false;
                break;
            case Abilities.TunnelingBarriers:
                EnableTunnelingBarriers = false;
                break;
            case Abilities.Superposition:
                EnableSuperposition = false;
                break;
            default:
                throw new System.ArgumentOutOfRangeException(nameof(ability), ability, null);
        }
    }
    
    public void ResetAbilities()
    {
        EnableDash = false;
        EnableAirDash = false;
        EnableWallJump = false;
        EnableDoubleJump = false;
        EnableEntanglementMode = false;
        EnableTunnelingBarriers = false;
        EnableSuperposition = false;
    }

    public bool HasAbility(Abilities ability)
    {
        switch (ability)
        {
            case Abilities.Dash:
               return EnableDash;
            case Abilities.AirDash:
                return EnableAirDash;
            case Abilities.DoubleJump:
                return EnableDoubleJump;
            case Abilities.WallJump:
                return EnableWallJump;
            case Abilities.EntanglementMode:
                return EnableEntanglementMode;
            case Abilities.TunnelingBarriers:
                return EnableTunnelingBarriers;
            case Abilities.Superposition:
                return EnableSuperposition;
            default:
                throw new System.ArgumentOutOfRangeException(nameof(ability), ability, null);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(m_dashDestination, DashCheckRadius);
    }
}
