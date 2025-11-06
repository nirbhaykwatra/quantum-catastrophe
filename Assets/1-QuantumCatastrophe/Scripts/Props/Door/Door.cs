using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public enum DoorOperationMode
{
    Manual,
    Automatic
}
public class Door : MonoBehaviour, IUnlockable, IInteractable
{
    public DoorOperationMode OperationMode;
    [SerializeField] private KeyItem RequiredItem;
    private int m_openTrigger = Animator.StringToHash("Open");
    private int m_closeTrigger = Animator.StringToHash("Close");
    private Animator m_animator;
    private bool m_isOpen = false;
    
    [SerializeField] private Collider2D m_doorCollider;
    [SerializeField] private Collider2D m_doorTrigger;
    
    private TextMeshProUGUI m_interactionText;
    
    [field: SerializeField]
    public bool IsLocked { get; private set; } = true;
    
    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_interactionText = GetComponentInChildren<TextMeshProUGUI>();
    }
    
    public void Interact(GameObject interactor)
    {
        TryUnlock(interactor);
    }

    #region Lock/Unlock

    public void TryUnlock(GameObject interactor)
    {
        if (IsUnlocked())
        {
            return;
        }
        if (interactor.GetComponent<PlayerController>() != null)
        {
            CharacterInventory inventory = interactor.GetComponent<CharacterInventory>();
            KeyItem requiredItem = (KeyItem)inventory.FindItemByName(RequiredItem.Name);
            if (inventory.HasItem(requiredItem))
            {
                Unlock();
                inventory.RemoveItem(requiredItem);
                NotificationManager.Instance.RequestNotification($"You used {requiredItem.Name}!", 2f, NotificationType.Success);
                return;
            }
            NotificationManager.Instance.RequestNotification($"You need {RequiredItem.Name} to open this door!", 2f, NotificationType.Error);
        }
    }

    public void TryRemoteUnlock()
    {
        if (IsUnlocked())
        {
            Open();
        }
        else
        {
            NotificationManager.Instance.RequestNotification($"You need to unlock the door connected to this object with {RequiredItem.Name}!", 2f, NotificationType.Error);
        }
    }
    
    [Button]
    public void Unlock()
    {
        if (!IsLocked) return;
        IsLocked = false;
        m_interactionText.text = "";
        Open();
    }
    public bool IsUnlocked() => !IsLocked;
    
    [Button]
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
        if (IsLocked) return;
        if (m_isOpen)
        {
            Close();
            return;
        }
        m_isOpen = true;
        m_animator.SetTrigger(m_openTrigger);
    }
    
    public void Close()
    {
        if (IsLocked) return;
        if (!m_isOpen)
        {
            Open();           
            return;           
        }
        m_isOpen = false;
        m_animator.SetTrigger(m_closeTrigger);
    }
    
    #endregion

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsLocked)
        {
            m_interactionText.text = "Locked!";
            return;
        }

        if (other.GetComponent<PlayerController>())
        { 
            if (OperationMode == DoorOperationMode.Automatic) Open();
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsLocked)
        {
            m_interactionText.text = "";
            return;
        }

        if (other.GetComponent<PlayerController>())
        {
            if (OperationMode == DoorOperationMode.Automatic) Close();
        }
    }
    
    // Animation Events
    public void OnDoorOpen()
    {
        m_doorCollider.enabled = false;
    }
    
    public void OnDoorClose()
    {
        m_doorCollider.enabled = true;
    }

    public void ChangeOperationMode(DoorOperationMode mode)
    {
        OperationMode = mode;
    }

}
