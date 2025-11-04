using System;
using UnityEngine;

public class CharacterInteraction : MonoBehaviour
{
    [SerializeField] private string interactableTag = "Interactable";
    [field: SerializeField]
    public bool CanInteract { get; set; } = false;
    private IInteractable m_interactable;
    
    public void Interact()
    {
        if (m_interactable != null) 
        {
            m_interactable.Interact(gameObject);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag(interactableTag) || other.GetComponent<IInteractable>() == null) return;
        CanInteract = true;
        m_interactable = other.GetComponent<IInteractable>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag(interactableTag) || other.GetComponent<IInteractable>() == null) return;
        CanInteract = false;
        m_interactable = null;
    }
}
