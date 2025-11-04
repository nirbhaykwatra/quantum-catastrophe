using UnityEngine;

public class Door : MonoBehaviour, IUnlockable, IInteractable
{
    [SerializeField] private Keycard Keycard;
    private int m_openTrigger = Animator.StringToHash("Open");
    private int m_closeTrigger = Animator.StringToHash("Close");
    private bool m_isOpen = false;
    private Animator m_animator;
    
    public void Interact(GameObject interactor)
    {
        TryUnlock(interactor);
    }

    public bool IsLocked { get; private set; }

    public void TryUnlock(GameObject interactor)
    {
        if (IsUnlocked())
        {
            return;
        }
        if (interactor.GetComponent<PlayerController>())
        {
            if (interactor.GetComponent<CharacterInventory>().Inventory.Contains(Keycard))
            {
                Unlock();
            }
        }
    }
    private void Unlock()
    {
        IsLocked = false;
        Open();
    }
    public bool IsUnlocked() => !IsLocked;
    
    public void Lock()
    {
        if (IsLocked) return;
        if (m_isOpen)
        {
            Close();
        }
        IsLocked = true;
    }

    public void Open()
    {
        m_isOpen = true;
        m_animator.SetTrigger(m_openTrigger);
    }
    
    public void Close()
    {
        m_isOpen = false;
        m_animator.SetTrigger(m_closeTrigger);
    }

}
