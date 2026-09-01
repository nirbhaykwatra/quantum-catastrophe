using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using QC.Systems.Notifications;
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

namespace QC.Character
{
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
        public float DashJumpBoostDuration { get; set; } = 0.2f;
        [field: SerializeField]
        public float DashJumpBoostMultiplier { get; set; } = 1.5f;
        [field: SerializeField]
        public LayerMask GroundMask { get; set; }
        
        [field: Header("Wall Jump Interactions")]
        [field: SerializeField] public bool RechargeDashOnWallJump { get; set; } = true;
        [field: SerializeField] public bool RechargeAirDashOnWallJump { get; set; } = true;
        
        [SerializeField] private PlayerData m_playerData;
        private UIEventBus _uiEventBus;
        
        // --- Dashing Variables ---
        
        [Title("Read-Only Fields")] 
        [ShowInInspector] [ReadOnly] public bool IsDashing { get; private set; }
        [ShowInInspector] [ReadOnly] public bool CanDash { get; set; } = true;
        [ShowInInspector] [ReadOnly] public bool CanAirDash { get; set; } = true;
        [ShowInInspector] [ReadOnly] public Vector2 DashDirection { get; set; }
        
        private Rigidbody2D m_rigidbody;
        private CharacterMovement2D m_movement;
        private float m_initialGravityScale;
        private float m_dashCooldownTimer;
        private Vector2 m_dashDestination;
        private Coroutine m_dashCoroutine;
        private bool m_dashCancelRequested;
        
        // --- Entanglement Variables ---
        
        private PlayerMode m_playerMode;
        [SerializeField] private float m_selectionRadius = 5f;
        [SerializeField] private LayerMask m_entanglableMask;
        
        private bool m_entanglementActive;
        
        private void OnEnable()
        {
            
        }

        private void Awake()
        {
            m_rigidbody = GetComponent<Rigidbody2D>();
            m_movement = GetComponent<CharacterMovement2D>();
            m_playerMode = PlayerMode.Normal;
            _uiEventBus = ServiceLocator.ForSceneOf(this).Get<EventBusRegistry>().Get<UIEventBus>();
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

        #region Dashing
        
        public void RechargeDashCooldown()
        {
            CancelActiveDash();
            m_dashCooldownTimer = DashCooldown;
            CanDash = true;
        }

        public void RechargeAirDashCooldown()
        {
            CancelActiveDash();
            CanAirDash = true;
        }

        private void CancelActiveDash()
        {
            m_dashCancelRequested = true;

            if (m_dashCoroutine != null)
            {
                StopCoroutine(m_dashCoroutine);
                m_dashCoroutine = null;
            }
            // Restore state ourselves — don't rely on the coroutine's own
            // cleanup code running, since Unity doesn't guarantee that when
            // a coroutine is stopped externally via StopCoroutine.
            m_rigidbody.gravityScale = m_initialGravityScale;
            m_movement.ForceUnlockAllMovement();
            IsDashing = false;
        }

        public void TryDash()
        {
            m_dashCoroutine = StartCoroutine(PerformDash(isAirDash: false));
        }

        public void TryAirDash()
        {
            m_dashCoroutine = StartCoroutine(PerformDash(isAirDash: true));
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

            m_dashCancelRequested = false;
            m_rigidbody.gravityScale = 0f;
            m_movement.CanMove = false;
            m_movement.CanTurn = false;
            IsDashing = true;

            float timer = 0f;
            float progress = 0f;

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
                if (m_dashCancelRequested) yield break; // cleanup already handled by CancelActiveDash

                timer += Time.deltaTime;
                progress = timer / duration;

                DashDirection = (destination - start).normalized;

                Vector2 position = Vector2.Lerp(start, destination, progress);
                m_rigidbody.MovePosition(position);
                
                yield return null;
            }

            StartCoroutine(DashJumpBoostWindow());
            
            // Natural completion — only reached if nobody cancelled us
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
        
        private IEnumerator DashJumpBoostWindow()
        {
            m_movement.CanDashJump = true;
            yield return new WaitForSeconds(DashJumpBoostDuration);
            m_movement.CanDashJump = false;
        }
            
        #endregion
        
        public void OnWallJump()
        {
            if (RechargeDashOnWallJump) RechargeDashCooldown();
            if (RechargeAirDashOnWallJump) RechargeAirDashCooldown();
        }
        
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
}


