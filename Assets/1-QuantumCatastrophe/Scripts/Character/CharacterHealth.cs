using System;
using GameEvents;
using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterHealth : MonoBehaviour
{
    [SerializeField] private PlayerData m_playerData;
    [SerializeField] private IntEventAsset OnDeathEvent;
    [SerializeField] private int MaxHealth = 5;
    [field: SerializeField]
    [ReadOnly]
    public int Health { get; private set; } = 5;
    public bool IsDead => Health <= 0;

    public Vector3 ResetPoint { get; private set; }
    
    public event Action<int> OnTakeDamage;
    public event Action<int> OnHeal; 
    public event Action OnDeath;

    private void Start()
    {
        m_playerData.Health = m_playerData.Health == 0 ? MaxHealth : m_playerData.Health;
        Health = m_playerData.Health;
    }

    private void OnDestroy()
    {
        m_playerData.Health = Health;
    }

    [Button]
    public void Damage(int amount)
    {
        if (IsDead) return;
        if (Health - amount <= 0)
        {
            Kill();
        }
        
        OnTakeDamage?.Invoke(amount);
        Health -= amount;
        m_playerData.Health = Health;
    }
    
    [Button]
    public void Heal(int amount)
    {
        if (Health + amount > MaxHealth) return;
        OnHeal?.Invoke(amount);
        Health += amount;
        m_playerData.Health = Health;
    }

    [Button]
    public void Kill()
    {
        OnDeath?.Invoke();
        //m_playerData.Health = MaxHealth;
        OnDeathEvent.Invoke(Health);
    }

    public void SetResetPoint(Vector3 point)
    {
        ResetPoint = point;
    }

    public void SetHealth(int health)
    {
        Health = health;
    }
}
