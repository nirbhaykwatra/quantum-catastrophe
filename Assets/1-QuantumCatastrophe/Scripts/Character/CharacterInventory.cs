using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Newtonsoft.Json;

namespace QC.Character
{
    public class CharacterInventory : MonoBehaviour
    {
        [ShowInInspector]
        private List<Loot> Inventory;

        private void Awake()
        {
            Inventory = new List<Loot>();
        }

        private void OnEnable()
        {
        
        }

        private void OnDestroy()
        {
        
        }

        public T FindItem<T>() where T : Loot
        {
            return Inventory.Find(loot => loot is T) as T;
        }

        public Loot FindItemByName(string itemName)
        {
            return Inventory.Find(x => x.Name == itemName);
        }
        public bool HasItem(Loot loot)
        {
            return Inventory.Contains(loot);
        }
    
        public void AddItem(Loot loot)
        {
            Inventory.Add(loot);
            NotificationManager.Instance.RequestNotification("Received " + loot.Name + "!", 3f, NotificationType.Success);
        }
    
        public void RemoveItem(Loot loot)
        {
            Inventory.Remove(loot);
            NotificationManager.Instance.RequestNotification("Removed " + loot.Name + " from inventory!", 3f, NotificationType.Success);
        }
    }
}
