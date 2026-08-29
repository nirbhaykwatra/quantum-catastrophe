using QC.Character;
using QC.Systems.Tutorials;
using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace QC.Props.QuantumObjects
{
    public class TunnelingActivator : MonoBehaviour
    {
        [SerializeField] [ShowInInspector] private LootEntry Item;
        [SerializeField] private TutorialSequenceSO _tutorialSequence;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.GetComponent<PlayerController>())
            {
                CharacterInventory inventory = other.gameObject.GetComponent<CharacterInventory>();
                inventory.AddItems(Item);
                ServiceLocator.ForSceneOf(this).Get<EventBusRegistry>().Get<UIEventBus>().Raise(new OnRequestTutorialEvent { Sequence = _tutorialSequence });
                Destroy(gameObject);
            }
        }
    }
}