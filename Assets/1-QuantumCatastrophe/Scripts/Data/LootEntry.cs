using System;
using Sirenix.OdinInspector;
using UnityEngine;

public enum LootType
{
    Collectible,
    KeyItem
}

[Serializable]
public struct LootEntry
{
    public Loot Item;
    public int Quantity;
    public LootType Type;
}
