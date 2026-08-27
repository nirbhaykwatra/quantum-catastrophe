using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Newtonsoft.Json;

namespace QC.Character
{
    [Serializable]
    public class InventorySlot
    {
        public Loot Item;
        public int Quantity;

        public InventorySlot(Loot item, int quantity = 1)
        {
            Item = item;
            Quantity = quantity;
        }
    }

    public class CharacterInventory : MonoBehaviour
    {
        [ShowInInspector]
        [SerializeField]
        private List<InventorySlot> Inventory = new();

        public InventorySlot FindSlot(Loot item)
        {
            return Inventory.Find(slot => slot.Item == item);
        }

        public InventorySlot FindSlotByName(string itemName)
        {
            return Inventory.Find(slot => slot.Item.Name == itemName);
        }

        // Keep old API working
        public Loot FindItem(Loot item) => FindSlot(item)?.Item;
        public Loot FindItemByName(string itemName) => FindSlotByName(itemName)?.Item;

        public bool HasItem(Loot loot)
        {
            InventorySlot slot = FindSlot(loot);
            return slot != null && slot.Quantity > 0;
        }

        /// <summary>Adds a single unit of a loot item, stacking if it already exists.</summary>
        public void AddItem(Loot loot, int quantity = 1)
        {
            InventorySlot slot = FindSlot(loot);
            if (slot != null)
            {
                slot.Quantity += quantity;
            }
            else
            {
                Inventory.Add(new InventorySlot(loot, quantity));
            }
        }

        /// <summary>Adds multiple loot entries at once, stacking duplicates.</summary>
        public void AddItems(IEnumerable<LootEntry> entries)
        {
            foreach (LootEntry entry in entries)
            {
                if (entry.Item == null || entry.Quantity <= 0) continue;
                AddItem(entry.Item, entry.Quantity);
            }
        }

        public void RemoveItem(Loot loot, int quantity = 1)
        {
            InventorySlot slot = FindSlot(loot);
            if (slot == null) return;

            slot.Quantity -= quantity;
            if (slot.Quantity <= 0)
                Inventory.Remove(slot);
        }
    }
}