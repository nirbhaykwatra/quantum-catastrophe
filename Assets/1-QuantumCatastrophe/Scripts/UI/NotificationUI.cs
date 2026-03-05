using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationUI : MonoBehaviour
{
    private Animator m_animator;
    private TextMeshProUGUI m_text;
    private float m_duration;
    private Image m_background;
    private NotificationType m_type;

    private float m_timer;
    private bool m_created = false;
    
    private void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!m_created) return;
        
        if (m_timer > 0)
        {
            m_timer -= Time.deltaTime;
        }
        else if (m_timer <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void InstantiateNotification(Notification notification)
    {
        InstantiateNotification(notification.Message, notification.Duration, notification.Type);
    }

    public void InstantiateNotification(string message, float duration, NotificationType type)
    {
        m_text = GetComponentInChildren<TextMeshProUGUI>();
        m_background = GetComponentInChildren<Image>();
        m_text.text = message;
        m_duration = duration;
        m_type = type;
        
        m_timer = m_duration;

        m_background.color = m_type switch
        {
            NotificationType.Error => Color.red,
            NotificationType.Success => Color.green,
            NotificationType.Info => Color.white,
            NotificationType.Warning => Color.yellow,
            _ => Color.white
        };
        m_created = true;
    }

    public void ShowNotification(string message)
    {
        m_text.text = message;
        m_animator.SetTrigger("Show");
    }
    
    public void HideNotification()
    {
        m_animator.SetTrigger("Hide");
    }
}
