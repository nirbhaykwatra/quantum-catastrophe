using System;
using UnityEngine;

public class EnergyBarrier : MonoBehaviour
{
    [SerializeField] private GameObject m_collider;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player)
        {
            CharacterAbilities abilities = player.GetComponent<CharacterAbilities>();
            
            if (abilities.IsDashing) m_collider.SetActive(false);
            
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player)
        {
            m_collider.SetActive(true);
            CharacterAbilities abilities = player.GetComponent<CharacterAbilities>();
            abilities.RechargeDashCooldown();
            abilities.RechargeAirDashCooldown();
        }
    }
}
