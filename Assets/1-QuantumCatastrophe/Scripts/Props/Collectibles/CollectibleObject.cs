using System;
using UnityEngine;

public class CollectibleObject : MonoBehaviour
{
    [SerializeField] private Loot Item;
    private Animator m_animator;
    
    private void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<PlayerController>())
        {
            CharacterInventory inventory = other.gameObject.GetComponent<CharacterInventory>();
            inventory.AddItem(Item);
            Destroy(gameObject);
        }
    }
}
