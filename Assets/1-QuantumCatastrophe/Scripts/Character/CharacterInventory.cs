using System.Collections.Generic;
using UnityEngine;

public class CharacterInventory : MonoBehaviour
{
    public List<Loot> Inventory = new List<Loot>();
    
    public void AddItem(Loot loot)
    {
        Inventory.Add(loot);
    }
    
    public void RemoveItem(Loot loot)
    {
        Inventory.Remove(loot);
    }
}
