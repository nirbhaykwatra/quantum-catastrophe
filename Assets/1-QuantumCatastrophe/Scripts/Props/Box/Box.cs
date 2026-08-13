using System.Collections.Generic;
using QC.Character;
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

    [SerializeField] private List<Loot> Items;
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
    
    public void Interact(GameObject interactor)
    {
        if (interactor.GetComponent<PlayerController>())
        {
            if (!m_isOpen)
            {
                if (Items.Count == 0)
                {
                    Open();
                    return;
                }
                
                Open();

                for (int i = Items.Count - 1; i >= 0; i--)
                {
                    interactor.GetComponent<CharacterInventory>().AddItem(Items[i]);
                    Items.RemoveAt(i);
                }
                
                if (SendMessageOnOpen)
                {
                    NotificationManager.Instance.RequestModal(OpenMessage);
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
