using System;
using TMPro;
using UnityEngine;

public class Box : MonoBehaviour, IInteractable
{
    private int m_open = Animator.StringToHash("Open");
    private int m_close = Animator.StringToHash("Close");
    private Animator m_animator;
    private TextMeshProUGUI m_interactionText;
    private bool m_isOpen = false;

    [SerializeField] private Loot LootItem;

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
                m_animator.SetTrigger(m_open);
                m_isOpen = true;
                m_interactionText.text = "";
                interactor.GetComponent<CharacterInventory>().Inventory.Add(LootItem);
            }
            else
            {
                m_animator.SetTrigger(m_close);
            }
        }
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
