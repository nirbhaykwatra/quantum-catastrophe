using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;

[System.Flags]
public enum Abilities
{
    Dash = 1 << 0,
    AirDash = 1 << 1,
    WallJump = 1 << 2,
    DoubleJump = 1 << 3,
    EntanglementMode = 1 << 4,
    TunnelingBarriers = 1 << 5,
    Superposition = 1 << 6,
}

public enum PlayerMode
{
    Normal,
    Entangle
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
    
    [SerializeField] private PlayerData m_playerData;
    private GlobalEventBus m_globalEventBus;
    
    // --- Dashing Variables ---
    
    [Title("Read-Only Fields")] 
    [ShowInInspector] [ReadOnly] public bool IsDashing { get; private set; }
    [ShowInInspector] [ReadOnly] public bool CanDash { get; set; } = true;
    [ShowInInspector] [ReadOnly] public bool CanAirDash { get; set; }
    [ShowInInspector] [ReadOnly] public Vector2 DashDirection { get; set; }
    
    private Rigidbody2D m_rigidbody;
    private CharacterMovement2D m_movement;
    private float m_initialGravityScale;
    private float m_dashCooldownTimer;
    private Vector2 m_dashDestination;
    
    // --- Entanglement Variables ---
    
    private PlayerMode m_playerMode;
    [SerializeField] private float m_selectionRadius = 5f;
    [SerializeField] private LayerMask m_entanglableMask;

    private IEntangleable m_firstTarget;
    private IEntangleable m_secondTarget;
    private bool m_entanglementActive;

    // Currently linked pairs
    private readonly List<(IEntangleable, IEntangleable)> m_pairs = new();

    private void OnEnable()
    {
        m_globalEventBus = ServiceLocator.Global.Get<EventBusRegistry>().Get<GlobalEventBus>();
    }

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_movement = GetComponent<CharacterMovement2D>();
        m_playerMode = PlayerMode.Normal;
    }

    private void Start()
    {
        IsDashing = false;
        m_dashCooldownTimer = DashCooldown;
        m_initialGravityScale = m_rigidbody.gravityScale;
        
#if !UNITY_EDITOR
        EnableDash = PlayerPrefs.GetInt("EnableDash", 0) == 1;
        EnableAirDash = PlayerPrefs.GetInt("EnableAirDash", 0) == 1;
        EnableWallJump = PlayerPrefs.GetInt("EnableWallJump", 0) == 1;
        EnableDoubleJump = PlayerPrefs.GetInt("EnableDoubleJump", 0) == 1;
        EnableEntanglementMode = PlayerPrefs.GetInt("EnableEntanglementMode", 0) == 1;
        EnableTunnelingBarriers = PlayerPrefs.GetInt("EnableTunnelingBarriers", 0) == 1;
        EnableSuperposition = PlayerPrefs.GetInt("EnableSuperposition", 0) == 1;
#endif
    }

    private void Update()
    {
        if (!CanDash)
        {
            m_dashCooldownTimer -= Time.deltaTime;
            if (m_dashCooldownTimer <= 0)
            {
                CanDash = true;
            }
        }
    }

    #region Entanglement

    public PlayerMode GetPlayerMode() => m_playerMode;

    public void ChangePlayerMode(PlayerMode mode)
    {
        m_playerMode = mode;
        m_globalEventBus.Raise(new OnModeChange { Mode = mode });
        m_entanglementActive = mode == PlayerMode.Entangle;
        if (m_entanglementActive)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            CancelSelection();
        }
        Debug.Log($"Player mode changed to {mode}");
    }

    public void TrySelect(Vector2 position)
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(position);
        Collider2D hit = Physics2D.OverlapPoint(mousePos);
        if (hit != null)
        {
            IEntangleable entangleable = hit.GetComponent<IEntangleable>();
            TryEntangle(entangleable);
        }
    }
    
    public void TryEntangle(IEntangleable target)
    {
        if (!m_entanglementActive || target == null) return;

        if (m_firstTarget == null)
        {
            m_firstTarget = target;
            m_firstTarget.OnEntanglementSelected();
        }
        else if (m_secondTarget == null && target != m_firstTarget)
        {
            m_secondTarget = target;
            m_secondTarget.OnEntanglementSelected();
            CommitEntanglement();
        }

        if (m_firstTarget != null && m_secondTarget != null)
        {
            if (target == m_firstTarget)
            {
                CancelSelection();
            }
            else if (target == m_secondTarget)
            {
                m_secondTarget.OnEntanglementDeselected();
                m_secondTarget = null;
            }
        }
    }

    private void CommitEntanglement()
    {
        m_firstTarget.OnEntangle(m_secondTarget);
        m_secondTarget.OnEntangle(m_firstTarget);
        m_pairs.Add((m_firstTarget, m_secondTarget));
        CancelSelection();
    }

    public void CancelSelection()
    {
        m_firstTarget?.OnEntanglementDeselected();
        m_secondTarget?.OnEntanglementDeselected();
        m_firstTarget = null;
        m_secondTarget = null;
    }

    #endregion
    
    

    #region Dashing
    
    public void RechargeDashCooldown()
    {
        IsDashing = false;
        StopCoroutine(PerformDash(isAirDash: false));
        m_dashCooldownTimer = DashCooldown;
    }
    
    public void RechargeAirDashCooldown()
    {
        IsDashing = false;
        StopCoroutine(PerformDash(isAirDash: true));
        CanAirDash = true;
    }
    
    public void TryDash()
    {
        StartCoroutine(PerformDash(isAirDash: false));
    }

    public void TryAirDash()
    {
        StartCoroutine(PerformDash(isAirDash: true));
    }

    public void OnGrounded()
    {
        RechargeAirDashCooldown();
    }

    public IEnumerator PerformDash(bool isAirDash = false)
    {
        if (IsDashing) yield break;
        if (isAirDash)
        {
            if (!EnableAirDash) yield break;
            if (!CanAirDash) yield break;
        }
        else
        {
            if (!EnableDash) yield break;
            if (!CanDash) yield break;
        }

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
            
            DashDirection = (destination - start).normalized;
            
            Vector2 position = Vector2.Lerp(start, destination, progress);
            m_rigidbody.MovePosition(position);
            
            yield return null;
        }
        
        m_rigidbody.gravityScale = m_initialGravityScale;
        m_movement.CanMove = true;
        m_movement.CanTurn = true;
        if (!isAirDash)
        {
            m_dashCooldownTimer = DashCooldown;
            CanDash = false;
        }
        else
        {
            CanAirDash = false;
        }
        IsDashing = false;
    }
        
    #endregion
    
    #region Ability API

    public void UnlockAbility(Abilities ability)
    {
        switch (ability)
        {
            case Abilities.Dash:
                EnableDash = true;
                PlayerPrefs.SetInt("EnableDash", 1);
                break;
            case Abilities.AirDash:
                EnableAirDash = true;
                PlayerPrefs.SetInt("EnableAirDash", 1);
                break;
            case Abilities.WallJump:
                EnableWallJump = true;
                PlayerPrefs.SetInt("EnableWallJump", 1);
                break;
            case Abilities.DoubleJump:
                EnableDoubleJump = true;
                PlayerPrefs.SetInt("EnableDoubleJump", 1);
                break;
            case Abilities.EntanglementMode:
                EnableEntanglementMode = true;
                PlayerPrefs.SetInt("EnableEntanglementMode", 1);
                break;
            case Abilities.TunnelingBarriers:
                EnableTunnelingBarriers = true;
                PlayerPrefs.SetInt("EnableTunnelingBarriers", 1);
                break;
            case Abilities.Superposition:
                EnableSuperposition = true;
                PlayerPrefs.SetInt("EnableSuperposition", 1);
                break;
            default:
                throw new System.ArgumentOutOfRangeException(nameof(ability), ability, null);
        }
        PlayerPrefs.Save();
    }

    public void LockAbility(Abilities ability)
    {
        switch (ability)
        {
            case Abilities.Dash:
                EnableDash = false;
                PlayerPrefs.SetInt("EnableDash", 0);
                break;
            case Abilities.AirDash:
                EnableAirDash = false;
                PlayerPrefs.SetInt("EnableAirDash", 0);
                break;
            case Abilities.WallJump:
                EnableWallJump = false;
                PlayerPrefs.SetInt("EnableWallJump", 0);
                break;
            case Abilities.DoubleJump:
                EnableDoubleJump = false;
                PlayerPrefs.SetInt("EnableDoubleJump", 0);
                break;
            case Abilities.EntanglementMode:
                EnableEntanglementMode = false;
                PlayerPrefs.SetInt("EnableEntanglementMode", 0);
                break;
            case Abilities.TunnelingBarriers:
                EnableTunnelingBarriers = false;
                PlayerPrefs.SetInt("EnableTunnelingBarriers", 0);
                break;
            case Abilities.Superposition:
                EnableSuperposition = false;
                PlayerPrefs.SetInt("EnableSuperposition", 0);
                break;
            default:
                throw new System.ArgumentOutOfRangeException(nameof(ability), ability, null);
        }
        PlayerPrefs.Save();
    }
    
    public void ResetAbilities()
    {
        foreach (Abilities ability in System.Enum.GetValues(typeof(Abilities)))
        {
            LockAbility(ability);
        }
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
    
    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(m_dashDestination, DashCheckRadius);
    }
}
