using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInteraction : MonoBehaviour
{
    [SerializeField] private string interactableTag = "Interactable";
    [field: SerializeField]
    public bool CanInteract { get; set; } = false;
    private List<IInteractable> m_interactables = new List<IInteractable>();
    
    public void Interact()
    {
        if (m_interactables.Count > 0 && CanInteract) 
        {
            foreach (IInteractable interactable in m_interactables)
            {
                interactable.Interact(new InteractionContext(gameObject));
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag(interactableTag) || other.GetComponent<IInteractable>() == null) return;
        CanInteract = true;
        m_interactables.Add(other.GetComponent<IInteractable>());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag(interactableTag) || other.GetComponent<IInteractable>() == null) return;
        CanInteract = false;
        m_interactables.Remove(other.GetComponent<IInteractable>());
    }
}
