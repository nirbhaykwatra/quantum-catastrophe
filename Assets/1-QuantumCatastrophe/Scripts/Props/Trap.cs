using UnityEngine;
using UnityEngine.Events;

public class Trap : MonoBehaviour
{
    [SerializeField] private int DamageAmount = 1;
    [SerializeField] private float PushbackForce = 10f;
    [SerializeField] private float SpeedMultiplier = 1f;
    [SerializeField] private bool IsActive = false;
    [SerializeField] private bool IsArmed = false;
    [SerializeField] private bool DamagePlayer = true;
    [SerializeField] private bool PushPlayer = true;
    
    private Animator m_animator;
    private PolygonCollider2D m_collider;
    
    public UnityEvent OnTrapTriggered;
    public UnityEvent OnTrapDeactivated;
    
    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_collider = GetComponent<PolygonCollider2D>();
        m_animator.speed *= SpeedMultiplier;
        ToggleTrap(IsActive);
        ToggleArmed(IsArmed);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.GetComponent<PlayerController>())
        {
            CharacterHealth health = other.gameObject.GetComponent<CharacterHealth>();
            if (DamagePlayer) health.Damage(DamageAmount);
            if (PushPlayer) Push(other.rigidbody, PushbackForce, other);
        }
    }

    private void Push(Rigidbody2D entity, float force, Collision2D collision)
    {
        Vector3 pushDirection = collision.contacts[0].normal;
        pushDirection.Normalize();
        entity.linearVelocity = Vector2.zero;
        entity.AddForce(-pushDirection * force, ForceMode2D.Impulse);
    }
    
    public void ToggleTrap(bool active)
    {
        IsActive = active;
        m_animator.SetBool("IsActivated", active);
    }
    
    public void ToggleArmed(bool armed)
    {
        IsArmed = armed;
        m_animator.SetBool("IsArmed", armed);
        m_collider.enabled = armed;
    }

    public void HandleTrapActivation()
    {
        OnTrapTriggered.Invoke();
    }
    
    public void HandleTrapDeactivation()
    {
        OnTrapDeactivated.Invoke();
    }
    
    
}
