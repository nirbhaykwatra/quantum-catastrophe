using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public enum ControlStationAction
{
    GivePlayerItem,
    GivePlayerAbility,
    WorldInteraction
}

public enum ConditionType
{
    None,
    RequireInventoryItems,
    RequireAbility,
    Custom
}

public class ControlStation : MonoBehaviour, IInteractable
{
    [Title("Settings")]
    [SerializeField]
    private ControlStationAction ControlStationAction = ControlStationAction.GivePlayerItem;
    private int m_openTrigger = Animator.StringToHash("Open");
    private int m_closeTrigger = Animator.StringToHash("Close");
    private Animator m_animator;
    private TextMeshProUGUI m_interactionText;
    private bool m_isOpen = false;
    
    [Title("Conditions")]
    [SerializeField]
    private ConditionType conditionType = ConditionType.None;

    [ShowIf("conditionType", ConditionType.RequireInventoryItems)]
    [SerializeField]
    private InventoryCondition inventoryCondition;

    [ShowIf("conditionType", ConditionType.RequireAbility)]
    [SerializeField]
    private AbilityCondition abilityCondition;

    [ShowIf("conditionType", ConditionType.Custom)]
    [SerializeField]
    private CustomCondition customCondition;
    
    [Title("Actions")]
    [ShowIf("ControlStationAction", ControlStationAction.WorldInteraction)]
    [SerializeField]
    private UnityEvent OnWorldInteract;
    
    [ShowIf("ControlStationAction", ControlStationAction.GivePlayerItem)]
    [SerializeField]
    private List<Loot> ItemsToGive = new List<Loot>();

    [ShowIf("ControlStationAction", ControlStationAction.GivePlayerAbility)]
    [SerializeField]
    private Abilities ability;
    
    [Title("Already Used")]
    [SerializeField]
    private bool hasBeenUsed = false;
    
    [ShowIf("hasBeenUsed")]
    [SerializeField]
    private string usedMessage = "Already activated.";

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_interactionText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Interact(GameObject interactor)
    {
        if (hasBeenUsed)
        {
            ShowMessage(usedMessage);
            return;
        }
        
        if (!CheckConditions(interactor))
        {
            return;
        }
        
        switch (ControlStationAction)
        {
            case ControlStationAction.GivePlayerAbility:
                CharacterAbilities abilities = interactor.GetComponent<CharacterAbilities>();
                CharacterSpawn spawn = interactor.GetComponent<CharacterSpawn>();
                foreach (Abilities abilityFlag in System.Enum.GetValues(typeof(Abilities)))
                {
                    if (ability.HasFlag(abilityFlag))
                    {
                        abilities.UnlockAbility(abilityFlag);
                    }
                }
                NotificationManager.Instance.RequestNotification("Unlocked " + ability + " ability!", 5f, NotificationType.Success);
                break;
            case ControlStationAction.GivePlayerItem:
                CharacterInventory inventory = interactor.GetComponent<CharacterInventory>();
                for (int i = ItemsToGive.Count - 1; i >= 0; i--)
                {
                    inventory.AddItem(ItemsToGive[i]);
                    ItemsToGive.RemoveAt(i);
                }
                break;
            case ControlStationAction.WorldInteraction:
                OnWorldInteract?.Invoke();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        Close();
        hasBeenUsed = true;
    }
    
    private bool CheckConditions(GameObject interactor)
    {
        ICondition condition = conditionType switch
        {
            ConditionType.RequireInventoryItems => inventoryCondition,
            ConditionType.RequireAbility => abilityCondition,
            ConditionType.Custom => customCondition,
            _ => null
        };

        if (condition != null && !condition.IsConditionMet(interactor))
        {
            ShowMessage(condition.GetFailureMessage());
            return false;
        }
        if (condition != null) condition.PostConditionCheck(interactor);
        ShowMessage(condition.GetSuccessMessage());
        return true;
    }

    private void ShowMessage(string message)
    {
        NotificationManager.Instance.RequestModal(message);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenUsed) return;
        if (other.GetComponent<PlayerController>())
        {
            Open();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (hasBeenUsed) return;
        if (other.GetComponent<PlayerController>())
        {
            Close();
        }
    }

    private void Open()
    {
        m_animator.SetTrigger(m_openTrigger);
        m_isOpen = true;
        m_interactionText.text = "Press E to activate";
    }
    
    private void Close()
    {
        m_animator.SetTrigger(m_closeTrigger);
        m_isOpen = false;
        m_interactionText.text = "";
    }
}
