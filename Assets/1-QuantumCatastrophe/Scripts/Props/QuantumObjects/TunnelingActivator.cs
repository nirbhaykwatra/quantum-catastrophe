using QC.Character;
using UnityEngine;

namespace QC.Props.QuantumObjects
{
    public class TunnelingActivator : MonoBehaviour
    {
        [SerializeField] private Loot ActivatorItem;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.GetComponent<PlayerController>())
            {
                CharacterInventory inventory = other.gameObject.GetComponent<CharacterInventory>();
                inventory.AddItem(ActivatorItem);
                Destroy(gameObject);
            }
        }
    }
}