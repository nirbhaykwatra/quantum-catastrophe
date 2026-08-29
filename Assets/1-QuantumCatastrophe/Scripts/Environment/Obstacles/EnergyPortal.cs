using QC.Character;
using UnityEngine;

public enum PortalMode
{
    Sending,
    Receiving
}

public enum PortalEjectionSide
{
    Left,
    Right
}

public class EnergyPortal : MonoBehaviour
{
    [SerializeField] private PortalMode PortalMode;
    [SerializeField] private EnergyPortal ConnectedPortal;
    [SerializeField] private PortalEjectionSide EjectionSide;
    
    [SerializeField] private float EjectionForce;
    [SerializeField] private GameObject m_collider;

    [SerializeField] private Transform _spawnPointLeft;
    [SerializeField] private Transform _spawnPointRight;
    
    private EnergyBarrierController m_controller;
    private bool m_forceAppliedThisDash;
    
    private void Awake()
    {
        m_controller = GetComponentInChildren<EnergyBarrierController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<CharacterAbilities>() != null)
        {
            if (!other.gameObject.GetComponent<CharacterAbilities>().EnableTunnelingBarriers) return;
            if (PortalMode == PortalMode.Receiving)
            {
                AddForceInDashDirection(other);
                return;
            }
            
            TeleportToConnectedPortal(other);
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<CharacterAbilities>() != null)
        {
            if (!other.gameObject.GetComponent<CharacterAbilities>().EnableTunnelingBarriers) return;
            AddForceInDashDirection(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<CharacterAbilities>() != null)
        {
            if (!other.gameObject.GetComponent<CharacterAbilities>().EnableTunnelingBarriers) return;
            ResetDashAndJump(other);
        }
    }

    public Vector2 GetSpawnPoint() => EjectionSide == PortalEjectionSide.Left ? _spawnPointLeft.position : _spawnPointRight.position;

    public Vector2 GetPortalFacingDirection() => EjectionSide == PortalEjectionSide.Left ? Vector2.left : Vector2.right;

    private void ResetDashAndJump(Collider2D other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player)
        {
            m_forceAppliedThisDash = false;
            m_controller.Activate();
            CharacterAbilities abilities = player.GetComponent<CharacterAbilities>();
            CharacterMovement2D movement = player.GetComponent<CharacterMovement2D>();
            movement.ResetMidAirJumpCount();
            abilities.RechargeDashCooldown();
            abilities.RechargeAirDashCooldown();
        }
    }
    
    private void AddForceInDashDirection(Collider2D other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player)
        {
            CharacterAbilities abilities = player.GetComponent<CharacterAbilities>();
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

            if (abilities.IsDashing)
            {
                m_forceAppliedThisDash = true;
                m_controller.Deactivate();
                rb.AddForce(abilities.DashDirection * EjectionForce, ForceMode2D.Impulse);
            }
        }
    }
    
    // TODO: This doesn't work but fuck it, later.
    private void TeleportToConnectedPortal(Collider2D other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player)
        {
            CharacterAbilities abilities = player.GetComponent<CharacterAbilities>();
            CharacterMovement2D movement = player.GetComponent<CharacterMovement2D>();
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

            if (abilities.IsDashing)
            {
                m_forceAppliedThisDash = true;
                m_controller.Deactivate();

                Vector2 targetPos = ConnectedPortal.GetSpawnPoint();
                rb.position = targetPos;
                Physics2D.SyncTransforms();       // force an immediate sync so colliders/raycasts see the new position this frame

                Debug.Log($"Teleport to: {targetPos}!");
                rb.AddForce(ConnectedPortal.GetPortalFacingDirection() * EjectionForce, ForceMode2D.Impulse);
                movement.SetLookDirection(ConnectedPortal.GetPortalFacingDirection());
            }
        }
    }
}