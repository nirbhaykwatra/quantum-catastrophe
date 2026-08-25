using System.Collections.Generic;
using QC.Character;
using UnityEngine;

[System.Serializable]
public class InventoryCondition : BaseCondition
{
    [SerializeField] private List<Loot> RequiredItems;
    [SerializeField] private bool DestroyItemsAfterUse = false;

    public override bool IsConditionMet(in InteractionContext context)
    {
        CharacterInventory inventory = context.Interactor.GetComponent<CharacterInventory>();
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

    public override void PostConditionCheck(in InteractionContext context)
    {
        if (!DestroyItemsAfterUse) return;
        
        CharacterInventory inventory = context.Interactor.GetComponent<CharacterInventory>();
        foreach (Loot item in RequiredItems)
        {
            if (inventory.HasItem(item))
            {
                inventory.RemoveItem(item);
            }
        }
    }
}