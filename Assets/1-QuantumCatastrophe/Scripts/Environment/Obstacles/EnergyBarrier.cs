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
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

            if (abilities.IsDashing)
            {
                m_collider.SetActive(false);
                rb.AddForce(abilities.DashDirection * 50f, ForceMode2D.Impulse);
            }
            
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
