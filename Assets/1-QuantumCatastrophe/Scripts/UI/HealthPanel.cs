using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthPanel : MonoBehaviour
{
    [SerializeField] 
    private Image m_healthPoint;
    
    private CharacterHealth m_health;

    private void OnEnable()
    {
        m_health = FindFirstObjectByType<CharacterHealth>();
        m_health.OnHeal += HandleHeal;
        m_health.OnTakeDamage += HandleDamage;
    }
    
    private void OnDisable()
    {
        m_health.OnHeal -= HandleHeal;
        m_health.OnTakeDamage -= HandleDamage;
    }

    private void Awake()
    {
        m_health = FindFirstObjectByType<CharacterHealth>();
    }

    private void Start()
    {
        for (int i = 0; i < m_health.Health; i++)
        {
            Instantiate(m_healthPoint, transform);
        }
    }

    private void HandleHeal(int amount)
    {
        Instantiate(m_healthPoint, transform);
    }
    
    private void HandleDamage(int amount)
    {
        if (transform.childCount == 0) return;
        for (int i = 0; i < amount; i++)
        {
            Destroy(transform.GetChild(0).gameObject);
        }
    }
}
