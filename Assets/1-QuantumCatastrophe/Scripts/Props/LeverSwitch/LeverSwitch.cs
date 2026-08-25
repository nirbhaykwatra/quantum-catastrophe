using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public enum SwitchOperationMode
{
    Toggle,
    Reset
}

public class LeverSwitch : MonoBehaviour, IInteractable
{
    private Animator m_animator;
    private TextMeshProUGUI m_interactionText;
    
    private int m_switchOnTrigger = Animator.StringToHash("SwitchOn");
    private int m_switchOffTrigger = Animator.StringToHash("SwitchOff");
    private int m_resetOnBool = Animator.StringToHash("Reset");
    private int m_isOnBool = Animator.StringToHash("IsOn");
    
    [ShowInInspector]
    private SwitchOperationMode OperationMode { get; set; }

    [SerializeField]
    [ShowIf("OperationMode", SwitchOperationMode.Reset)]
    private string NotificationMessage;
    [SerializeField]
    [ShowIf("OperationMode", SwitchOperationMode.Toggle)]
    private string SwitchOnNotificationMessage;
    [SerializeField]
    [ShowIf("OperationMode", SwitchOperationMode.Toggle)]
    private string SwitchOffNotificationMessage;
    private bool ResetOnUse { get; set; } = false;
    private bool SwitchedOn { get; set; } = false;
    
    [ShowIf("OperationMode", SwitchOperationMode.Toggle)]
    public UnityEvent OnSwitchOn;
    [ShowIf("OperationMode", SwitchOperationMode.Toggle)]
    public UnityEvent OnSwitchOff;
    [ShowIf("OperationMode", SwitchOperationMode.Reset)]   
    public UnityEvent OnSwitchReset;

    private void OnValidate()
    {
        ResetOnUse = OperationMode == SwitchOperationMode.Reset;
    }
    
    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_interactionText = GetComponentInChildren<TextMeshProUGUI>();
    }
    public void Interact(in InteractionContext context)
    {
        m_animator.SetBool(m_resetOnBool, ResetOnUse);
        if (!ResetOnUse)
        {
            if (SwitchedOn)
            {
                SwitchOff();
            }
            else
            {
                SwitchOn();
            }
        }
        else
        {
            SwitchOn();
        }
    }
    
    [Button]
    private void SwitchOn()
    {
        SwitchedOn = true;
        m_animator.SetTrigger(m_switchOnTrigger);
        m_animator.SetBool(m_isOnBool, SwitchedOn);
        if (OperationMode == SwitchOperationMode.Reset)
        {
            m_interactionText.text = "Press E to activate";
            OnSwitchReset?.Invoke();   
            NotificationManager.Instance.RequestNotification(NotificationMessage, 2f, NotificationType.Success);
        }
        else
        {
            m_interactionText.text = "Press E to deactivate";      
            OnSwitchOn?.Invoke();     
            NotificationManager.Instance.RequestNotification(SwitchOnNotificationMessage, 2f, NotificationType.Success);
        }
    }
    
    [Button]
    private void SwitchOff()
    {
        SwitchedOn = false;
        m_animator.SetTrigger(m_switchOffTrigger);
        m_animator.SetBool(m_isOnBool, SwitchedOn);
        m_interactionText.text = "Press E to activate";
        OnSwitchOff?.Invoke();       
        NotificationManager.Instance.RequestNotification(SwitchOffNotificationMessage, 2f, NotificationType.Success);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<PlayerController>())
        {
            if (ResetOnUse)
            {
                m_interactionText.text = "Press E to activate";
            }
            else
            {
                m_interactionText.text = SwitchedOn ? "Press E to deactivate" : "Press E to activate";
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<PlayerController>())
        {
            m_interactionText.text = "";
        }
    }

    public void ChangeOperationMode(SwitchOperationMode mode)
    {
        OperationMode = mode;
        ResetOnUse = mode == SwitchOperationMode.Reset;
        m_animator.SetBool(m_resetOnBool, ResetOnUse);
    }
}
