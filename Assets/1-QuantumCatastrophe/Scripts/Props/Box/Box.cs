using System.Collections.Generic;
using QC.Character;
using QC.Systems.Notifications;
using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class Box : MonoBehaviour, IInteractable
{
    private int m_open = Animator.StringToHash("Open");
    private int m_close = Animator.StringToHash("Close");
    private Animator m_animator;
    private TextMeshProUGUI m_interactionText;
    private bool m_isOpen = false;

    [SerializeField] private List<LootEntry> Items;
    [SerializeField] private bool SendMessageOnOpen = false;
    
    [ShowIf("SendMessageOnOpen")]
    [TextArea]
    [SerializeField]
    private string OpenMessage = "Box opened.";

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_interactionText = GetComponentInChildren<TextMeshProUGUI>();
    }
    
    public void Interact(in InteractionContext context)
    {
        if (context.Interactor.GetComponent<PlayerController>())
        {
            if (!m_isOpen)
            {
                if (Items.Count == 0)
                {
                    Open();
                    return;
                }
                
                Open();
                
                context.Interactor.GetComponent<CharacterInventory>().AddItems(Items);
                for (int i = Items.Count - 1; i >= 0; i--)
                {
                    if (SendMessageOnOpen)
                    {
                        ServiceLocator.ForSceneOf(this).Get<EventBusRegistry>().Get<UIEventBus>().Raise(new OnRequestNotification
                        {
                            Duration = 3f,
                            Icon = Items[i].Item.Icon,
                            Message = $"Received {Items[i].Quantity}x {Items[i].Item.Name}",
                            Type = NotificationType.Info
                        });
                    }
                    Items.RemoveAt(i);
                }
            }
            else
            {
                Close();
            }
        }
    }

    private void Open()
    {
        m_animator.SetTrigger(m_open);
        m_isOpen = true;
        m_interactionText.text = "";
    }
    
    private void Close()
    {
        m_animator.SetTrigger(m_close);
        m_isOpen = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() && !m_isOpen)
        {
            m_interactionText.text = "Press E to open";
        }
        else if (other.GetComponent<PlayerController>() && m_isOpen)
        {
            m_interactionText.text = "";
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() && !m_isOpen)
        {
            m_interactionText.text = "";
        }
    }
}
