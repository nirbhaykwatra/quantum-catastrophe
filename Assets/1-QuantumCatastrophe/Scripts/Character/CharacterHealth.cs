using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterHealth : MonoBehaviour
{
    [SerializeField] private int MaxHealth = 5;
    [field: SerializeField]
    [ReadOnly]
    public int Health { get; private set; } = 5;
    public bool IsDead => Health <= 0;

    public Vector3 ResetPoint { get; private set; }
    
    private event Action<int> OnTakeDamage;
    private event Action<int> OnHeal; 
    public event Action OnDeath;

    private void Awake()
    {
        Health = MaxHealth;
    }
    
    public void Damage(int amount)
    {
        if (IsDead) return;
        if (Health - amount <= 0)
        {
            Kill();
        }
        
        OnTakeDamage?.Invoke(amount);
        Health -= amount;
    }
    public void Heal(int amount)
    {
        OnHeal?.Invoke(amount);
        Health += amount;
    }

    public void Kill()
    {
        OnDeath?.Invoke();
    }

    public void SetResetPoint(Vector3 point)
    {
        ResetPoint = point;
    }
}
