using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

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
    //      
    
    // TODO: Fix dash cooldowns and air dash grounded reset
    
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
        if (CanDash) return;
        m_dashCooldownTimer -= Time.deltaTime;
        if (m_dashCooldownTimer <= 0)
        {
            CanDash = true;
            m_dashCooldownTimer = DashCooldown;
        }
    }

    public void TryDash()
    {
        Debug.Log("Trying dash");
        if (IsDashing || !CanDash) return;
        if (!m_movement.IsGrounded)
        {
            TryAirDash();
        }
        else
        {
            StartCoroutine(OnDash());
            CanDash = false;
            m_dashCooldownTimer = DashCooldown;
        }
    }

    public void TryAirDash()
    {
        Debug.Log("Trying air dash");
        if (!CanAirDash) return;
        StartCoroutine(OnDash());
        Debug.Log("Set air dash to false");
        CanAirDash = false;
        CanDash = false;
        m_dashCooldownTimer = DashCooldown;
    }

    public IEnumerator OnDash()
    {
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

    private void OnDrawGizmosSelected()
    {
        
    }
}
