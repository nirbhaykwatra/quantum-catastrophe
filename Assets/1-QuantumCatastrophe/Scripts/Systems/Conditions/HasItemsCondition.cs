using System.Collections.Generic;
using QC.Character;
using UnityEngine;

[System.Serializable]
public class InventoryCondition : BaseCondition
{
    [SerializeField] private List<LootEntry> RequiredItems;
    [SerializeField] private bool DestroyItemsAfterUse = false;

    public override bool IsConditionMet(in InteractionContext context)
    {
        CharacterInventory inventory = context.Interactor.GetComponent<CharacterInventory>();
        if (inventory == null) return false;

        foreach (LootEntry entry in RequiredItems)
        {
            InventorySlot slot = inventory.FindSlot(entry.Item);
            if (slot == null || slot.Quantity < entry.Quantity)
                return false;
        }
        return true;
    }

    public override void PostConditionCheck(in InteractionContext context)
    {
        if (!DestroyItemsAfterUse) return;

        CharacterInventory inventory = context.Interactor.GetComponent<CharacterInventory>();
        foreach (LootEntry entry in RequiredItems)
        {
            inventory.RemoveItem(entry);
        }
    }
}