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

    private void Awake()
    {
        int health = PlayerPrefs.GetInt("Health") == 0 ? MaxHealth : m_playerData.Health;
        PlayerPrefs.SetInt("Health", health);
        PlayerPrefs.Save();
        
        Health = PlayerPrefs.GetInt("Health", health);
    }

    private void OnDestroy()
    {
        PlayerPrefs.SetInt("Health", Health);
        PlayerPrefs.Save();
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
        PlayerPrefs.SetInt("Health", Health);
        PlayerPrefs.Save();
    }
    
    [Button]
    public void Heal(int amount)
    {
        if (Health + amount > MaxHealth) return;
        OnHeal?.Invoke(amount);
        Health += amount;
        PlayerPrefs.SetInt("Health", Health);
        PlayerPrefs.Save();
    }

    [Button]
    public void Kill()
    {
        OnDeath?.Invoke();
        OnDeathEvent.Invoke(Health);
    }

    public void SetResetPoint(Vector3 point)
    {
        ResetPoint = point;
    }

    public void SetHealth(int health)
    {
        Health = health;
        PlayerPrefs.SetInt("Health", Health);
        PlayerPrefs.Save();
    }
}
