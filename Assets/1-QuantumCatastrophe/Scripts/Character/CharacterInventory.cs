using System;
using System.Collections.Generic;
using FMOD;
using Sirenix.OdinInspector;
using UnityEngine;
using Newtonsoft.Json;
using QC.Props.QuantumObjects;
using Debug = UnityEngine.Debug;

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

        private CharacterAbilities _abilities;

        private void Awake()
        {
            _abilities = GetComponent<CharacterAbilities>();
        }

        public InventorySlot FindSlot(Loot item)
        {
            return Inventory.Find(slot => slot.Item == item);
        }

        public InventorySlot FindSlotByName(string itemName)
        {
            return Inventory.Find(slot => slot.Item.Name == itemName);
        }

        public bool HasItem(Loot loot)
        {
            InventorySlot slot = FindSlot(loot);
            return slot != null && slot.Quantity > 0;
        }

        /// <summary>Adds a single unit of a loot item, stacking if it already exists.</summary>
        private void AddItem(Loot loot, int quantity = 1)
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
                AddItems(entry);
            }
        }
        
        /// <summary>Adds a single LootEntry, stacking if the item already exists.</summary>
        public void AddItems(LootEntry entry)
        {
            if (entry.Item == null || entry.Quantity <= 0) return;
            if (entry.Type == LootType.KeyItem)
            {
                // TODO: Find a type safe way to do this, without the hard coded string
                if (entry.Item.Name == "TunnelingActivator")
                {
                    _abilities.EnableDash = true;
                    _abilities.EnableAirDash = true;
                    _abilities.EnableTunnelingBarriers = true;
                }
            }
            AddItem(entry.Item, entry.Quantity);
        }

        public void RemoveItem(LootEntry entry)
        {
            RemoveItem(entry.Item, entry.Quantity);
            if (entry.Type == LootType.KeyItem)
            {
                // TODO: Find a type safe way to do this, without the hard coded string
                if (entry.Item.Name == "TunnelingActivator")
                {
                    _abilities.EnableDash = false;
                    _abilities.EnableAirDash = false;
                    _abilities.EnableTunnelingBarriers = false;
                }
            }
        }

        private void RemoveItem(Loot loot, int quantity = 1)
        {
            InventorySlot slot = FindSlot(loot);
            if (slot == null) return;

            slot.Quantity -= quantity;
            if (slot.Quantity <= 0)
                Inventory.Remove(slot);
        }
    }
}