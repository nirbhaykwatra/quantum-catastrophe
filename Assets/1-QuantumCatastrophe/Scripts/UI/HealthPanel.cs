using System;
using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;
using UnityEngine;
using UnityEngine.UI;

public class HealthPanel : MonoBehaviour
{
    [SerializeField] 
    private Image m_healthPoint;
    
    private CharacterHealth m_health;
    private UIEventBus m_eventBus;
    
    // Event bindings
    private EventBinding<OnTakeDamage> m_onTakeDamage;
    private EventBinding<OnHeal> m_onHeal;

    private void OnEnable()
    {
        m_eventBus = ServiceLocator.Global.Get<EventBusRegistry>().Get<UIEventBus>();
        
        m_onTakeDamage = new EventBinding<OnTakeDamage>(HandleDamage);
        m_onHeal = new EventBinding<OnHeal>(HandleHeal);
        
        m_eventBus.Register(m_onTakeDamage);
        m_eventBus.Register(m_onHeal);
        
        m_health = FindFirstObjectByType<CharacterHealth>();
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

    private void HandleHeal(OnHeal @event)
    {
        Instantiate(m_healthPoint, transform);
    }
    
    private void HandleDamage(OnTakeDamage @event)
    {
        if (transform.childCount == 0) return;
        for (int i = 0; i < @event.Damage; i++)
        {
            Destroy(transform.GetChild(0).gameObject);
        }
    }
}
