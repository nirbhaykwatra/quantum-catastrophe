using System;
using GameEvents;
using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterHealth : MonoBehaviour
{
    [SerializeField] private PlayerData m_playerData;
    [SerializeField] private IntEventAsset OnDeathEvent;
    [SerializeField] private int MaxHealth = 5;
    [SerializeField] private float DamageCooldown = 1f;
    [field: SerializeField]
    [ReadOnly]
    public int Health { get; private set; } = 5;
    public bool IsDead => Health <= 0;

    public Vector3 ResetPoint { get; private set; }
    
    public event Action<int> OnTakeDamage;
    public event Action<int> OnHeal; 
    public event Action OnDeath;

    private bool m_wasDamaged;
    private float m_timer;

    private void Awake()
    {
        SetHealth(MaxHealth);
    }

    private void Update()
    {
        if (m_wasDamaged)
        {
            m_timer += Time.deltaTime;
            if (m_timer >= DamageCooldown)
            {
                m_wasDamaged = false;
                m_timer = 0;
            }
        }
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
        m_wasDamaged = true;
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
