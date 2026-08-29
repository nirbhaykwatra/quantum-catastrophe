using System;
using QC.Character;
using UnityEngine;

public class EnergyBarrier : MonoBehaviour
{
    [SerializeField] private float EjectionForce;
    [SerializeField] private GameObject m_collider;

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
            AddForceInDashDirection(other);
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
}
