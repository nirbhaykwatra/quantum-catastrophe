using System;
using UnityEngine;

[Serializable]
public struct LootEntry
{
    public Loot Item;
    [Min(1)]
    public int Quantity;
}
