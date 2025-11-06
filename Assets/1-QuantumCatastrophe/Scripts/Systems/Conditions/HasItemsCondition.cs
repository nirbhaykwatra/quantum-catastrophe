using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryCondition : ICondition
{
    [SerializeField] private List<Loot> RequiredItems;
    [SerializeField] private string FailureMessage = "You need specific items to use this.";
    [SerializeField] private bool DestroyItemsAfterUse = false;

    public bool IsConditionMet(GameObject interactor)
    {
        CharacterInventory inventory = interactor.GetComponent<CharacterInventory>();
        if (inventory == null) return false;

        foreach (Loot item in RequiredItems)
        {
            if (!inventory.HasItem(item))
            {
                return false;
            }
        }
        return true;
    }

    public void PostConditionCheck(GameObject interactor)
    {
        if (DestroyItemsAfterUse)
        {
            CharacterInventory inventory = interactor.GetComponent<CharacterInventory>();
            foreach (Loot item in RequiredItems)
            {
                if (inventory.HasItem(item))
                {
                    inventory.RemoveItem(item);
                }
            }
        }
    }

    public string GetFailureMessage() => FailureMessage;
}