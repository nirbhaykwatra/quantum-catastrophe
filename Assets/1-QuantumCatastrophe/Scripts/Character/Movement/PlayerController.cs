using System;
using System.Collections;
using System.Collections.Generic;
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

    protected Vector2 MoveInput { get; set; }

    protected virtual void OnValidate()
    {
        if(Movement == null) Movement = GetComponent<CharacterMovementBase>();
        if(Abilities == null) Abilities = GetComponent<CharacterAbilities>();
        if(Interaction == null) Interaction = GetComponent<CharacterInteraction>();
        if(Inventory == null) Inventory = GetComponent<CharacterInventory>();
    }

    protected virtual void Awake()
    {
        Cursor.lockState = CursorMode;
        if(Movement == null) Movement = GetComponent<CharacterMovementBase>();
        if(Abilities == null) Abilities = GetComponent<CharacterAbilities>();
        if(Interaction == null) Interaction = GetComponent<CharacterInteraction>();
        if(Inventory == null) Inventory = GetComponent<CharacterInventory>();
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
    }
}