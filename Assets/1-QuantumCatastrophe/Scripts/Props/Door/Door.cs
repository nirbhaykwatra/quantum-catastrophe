using System;
using System.Collections.Generic;
using System.Linq;
using QC.Character;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public enum DoorOperationMode
{
    Manual,
    Automatic
}

[Serializable]
public struct UnlockTriggerData
{
    public GameObject RequiredToUnlock;
    public Trigger TriggerToCheck;
}

namespace QC.Props
{
    public class Door : MonoBehaviour, IUnlockable, IInteractable
    {
        public DoorOperationMode OperationMode;
        
        [SerializeField] 
        [ShowInInspector] 
        [ShowIf("m_itemRequiredToUnlock")]
        private LootEntry RequiredItem;
        
        [SerializeField] 
        [ShowInInspector] 
        [ShowIf("m_remoteTriggerUnlock")]
        private List<UnlockTriggerData> TriggerData;
        
        private int m_openTrigger = Animator.StringToHash("Open");
        private int m_closeTrigger = Animator.StringToHash("Close");
        private Animator m_animator;
        [ShowInInspector] [ReadOnly] private bool m_isOpen = false;
        
        [SerializeField] private Collider2D m_doorCollider;
        [SerializeField] private Collider2D m_doorTrigger;
        
        [SerializeField] private bool m_destroyItemOnUse = true;
        [SerializeField] private bool m_itemRequiredToUnlock = false;
        [SerializeField] private bool m_remoteTriggerUnlock = false;
        
        private TextMeshProUGUI m_interactionText;
        private int _objectsRequiredAmount;
        private int _requiredObjectsTally;
        
        [field: SerializeField]
        public bool IsLocked { get; private set; } = true;
        
        private void Awake()
        {
            m_animator = GetComponent<Animator>();
            m_interactionText = GetComponentInChildren<TextMeshProUGUI>();

            _objectsRequiredAmount = TriggerData.Count;
        }

        private void FixedUpdate()
        {
            if (!m_remoteTriggerUnlock) return;

            _requiredObjectsTally = 0;

            foreach (UnlockTriggerData data in TriggerData)
            {
                if (IsRequiredObjectInTrigger(data.RequiredToUnlock, data.TriggerToCheck)) _requiredObjectsTally += 1;
            }
            
            if (_objectsRequiredAmount == _requiredObjectsTally)
            {
                if (IsUnlocked()) return;
                Unlock();
            }
            else
            {
                if (!IsUnlocked()) return;
                Lock();
            }
        }

        public void Interact(in InteractionContext context)
        {
            TryUnlock(context.Interactor);
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
                if (m_itemRequiredToUnlock)
                {
                    InventorySlot requiredItem = inventory.FindSlot(RequiredItem.Item);
                    if (requiredItem != null)
                    {
                        Unlock();
                        if (m_destroyItemOnUse) inventory.RemoveItem(RequiredItem);
                        // NotificationManager.Instance.RequestNotification($"You used {requiredItem.Name}!", 2f, NotificationType.Success);
                        return;
                    }
                    // NotificationManager.Instance.RequestNotification($"You need {RequiredItem.Name} to open this door!", 2f, NotificationType.Error);
                }
                Unlock();
                return;
            }
        }

        public void TryUnlock()
        {
            if (IsUnlocked())
            {
                return;
            }

            Unlock();
        }

        public void TryRemoteUnlock()
        {
            if (IsUnlocked())
            {
                Open();
            }
            else
            {
                // NotificationManager.Instance.RequestNotification($"You need to unlock the door connected to this object with {RequiredItem.Name}!", 2f, NotificationType.Error);
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
            if (m_isOpen) return;
            m_isOpen = true;
            m_animator.SetTrigger(m_openTrigger);
        }
        
        public void Close()
        {
            if (IsLocked) return;
            if (!m_isOpen) return;
            m_isOpen = false;
            m_animator.SetTrigger(m_closeTrigger);
        }

        [Button]
        public void ToggleDoor()
        {
            if (IsLocked) return;
            if (m_isOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        private bool IsRequiredObjectInTrigger(GameObject required, Trigger trigger)
        {
            return trigger.OverlappingObjects.Contains(required);
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
}
