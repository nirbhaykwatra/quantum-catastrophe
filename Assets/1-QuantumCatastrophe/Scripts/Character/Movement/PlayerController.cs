using System;
using System.Collections;
using System.Collections.Generic;
using QC.Character;
using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

// sends input from PlayerInput to attached CharacterMovement components
public class PlayerController : MonoBehaviour
{
    // initial cursor state
    [field: SerializeField] protected CursorLockMode CursorMode { get; set; } = CursorLockMode.Locked;
    // make character look in Camera direction instead of MoveDirection
    [field: SerializeField] protected bool LookInCameraDirection { get; set; }

    [field: Header("Components")]
    [field: SerializeField] protected CharacterMovementBase Movement { get; set; }
    [field: SerializeField] protected CharacterAbilities Abilities { get; set; }
    [field: SerializeField] protected CharacterInteraction Interaction { get; set; }
    [field: SerializeField] protected CharacterInventory Inventory { get; set; }
    
    private PlayerInput m_playerInput;
    private GlobalEventBus m_globalEventBus;

    protected Vector2 MoveInput { get; set; }
    
    private float m_disableTimer;
    private bool m_movementDisabled;

    protected virtual void OnValidate()
    {
        if(Movement == null) Movement = GetComponent<CharacterMovementBase>();
        if(Abilities == null) Abilities = GetComponent<CharacterAbilities>();
        if(Interaction == null) Interaction = GetComponent<CharacterInteraction>();
        if(Inventory == null) Inventory = GetComponent<CharacterInventory>();
        if(m_playerInput == null) m_playerInput = GetComponent<PlayerInput>();
    }

    protected virtual void Awake()
    {
        Cursor.lockState = CursorMode;
        if(Movement == null) Movement = GetComponent<CharacterMovementBase>();
        if(Abilities == null) Abilities = GetComponent<CharacterAbilities>();
        if(Interaction == null) Interaction = GetComponent<CharacterInteraction>();
        if(Inventory == null) Inventory = GetComponent<CharacterInventory>();
        if(m_playerInput == null) m_playerInput = GetComponent<PlayerInput>();
        
        m_globalEventBus = ServiceLocator.ForSceneOf(this).Get<EventBusRegistry>().Get<GlobalEventBus>();
    }

    private void OnEnable()
    {
        
    }

    private void Start()
    {
    }

    public virtual void OnMove(InputValue value)
    {
        MoveInput = value.Get<Vector2>();
    }

    public virtual void OnJump(InputValue value)
    {
        Movement?.TryJump();
    }

    public virtual void OnDash(InputValue value)
    {
        if (Abilities == null) return;
        if (Movement.IsGrounded)
        {
            Abilities?.TryDash();
        }
        else
        {
            Abilities?.TryAirDash();
        }
    }

    public virtual void OnInteract(InputValue value)
    {
        if (Interaction.CanInteract)
        {
            Interaction.Interact();
        }
    }

    public virtual void OnModeChange(InputValue value)
    { 
        m_globalEventBus.Raise(new OnToggleEntanglement());
    }

    public virtual void OnEntangleSelect(InputValue value)
    {
        m_globalEventBus.Raise(new OnClickEntangle());
    }

    public virtual void OnObserve(InputValue value)
    {
        m_globalEventBus.Raise(new OnToggleSuperposition { Observed = value.isPressed});
    }

    public virtual void OnZoomOut(InputValue value)
    {
        m_globalEventBus.Raise(new OnZoomOut());
    }

    public virtual void OnResetLevel(InputValue value)
    {
        m_globalEventBus.Raise(new OnResetLevel());
    }

    public virtual void OnPause(InputValue value)
    {
        m_globalEventBus.Raise(new OnPauseRequestedEvent());
    }

    protected virtual void Update()
    {
        if (Movement == null) return;

        // find correct right/forward directions based on main camera rotation
        Vector3 up = Vector3.up;
        Vector3 right = Camera.main.transform.right;
        Vector3 forward = Vector3.Cross(right, up);
        Vector3 moveInput = forward * MoveInput.y + right * MoveInput.x;

        // send player input to character movement
        Movement.SetMoveInput(moveInput);
        Movement.SetLookDirection(moveInput);
        if (LookInCameraDirection) Movement.SetLookDirection(Camera.main.transform.forward);

        if (m_movementDisabled && m_disableTimer > 0)
        {
            m_disableTimer -= Time.deltaTime;
            if (m_disableTimer <= 0)
            {
                EnableMovement();
            }
        }
        
    }
    public void DisableMovement(float timer)
    {
        m_movementDisabled = true;
        m_disableTimer = timer;
        m_playerInput.DeactivateInput();
        Debug.Log($"Deactivating input for {timer} seconds");
    }
    
    public void EnableMovement()
    {
        m_movementDisabled = false;
        m_playerInput.ActivateInput();
        Debug.Log("Activating input");
    }
}