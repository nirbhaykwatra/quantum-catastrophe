using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterInventory : MonoBehaviour
{
    [ShowInInspector]
    private List<Loot> Inventory;

    private void Awake()
    {
        Inventory = new List<Loot>();
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
    }
    
    public void RemoveItem(Loot loot)
    {
        Inventory.Remove(loot);
    }
}
