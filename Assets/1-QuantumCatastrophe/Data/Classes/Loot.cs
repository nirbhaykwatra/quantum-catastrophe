using UnityEngine;

public class Loot : ScriptableObject
{
    public string Name;
    public Sprite Icon;
    
    public virtual void Use()
    {
        Destroy(this);
    }
}
