using System;
using GameEvents;
using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;
using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterHealth : MonoBehaviour
{
    [SerializeField] private PlayerData m_playerData;
    [SerializeField] private IntEventAsset OnDeathEvent;
    [SerializeField] private BoolEventAsset OnRespawnEvent;
    [SerializeField] private int MaxHealth = 5;
    [SerializeField] private float DamageCooldown = 1f;
    [field: SerializeField]
    [ReadOnly]
    public int Health { get; private set; } = 5;
    
    public int MaxHealthValue => MaxHealth;
    public bool IsDead => Health <= 0;
    
    private UIEventBus m_eventBus;

    private bool m_wasDamaged;
    private float m_timer;
    
    private void Awake()
    {
        m_eventBus = ServiceLocator.ForSceneOf(this).Get<EventBusRegistry>().Get<UIEventBus>();
    }

    private void OnEnable()
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
        
        m_eventBus.Raise(new OnTakeDamage { Damage = amount });
        SetHealth(Health - amount);
        m_wasDamaged = true;
        PlayerPrefs.SetInt("Health", Health);
        PlayerPrefs.Save();
    }
    
    [Button]
    public void Heal(int amount)
    {
        m_eventBus.Raise(new OnHeal { Amount = amount });
        SetHealth(Health + amount);
        PlayerPrefs.SetInt("Health", Health);
        PlayerPrefs.Save();
    }

    [Button]
    public void Kill()
    {
        m_eventBus.Raise(new OnDeath());
    }

    public void SetHealth(int health)
    {
        Health = health;
        m_eventBus.Raise(new OnHealthChangedEvent { Current = health, Max = MaxHealth});
        PlayerPrefs.SetInt("Health", Health);
        PlayerPrefs.Save();
    }
}
