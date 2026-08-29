using System;
using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace QC.Systems.Tutorials
{
    public class TutorialTrigger : MonoBehaviour
    {
        [SerializeField] private TutorialSequenceSO sequence;
        [SerializeField] private bool fireOnce = true;
        [SerializeField] private bool triggerWorldEvent;
        
        [ShowIf("triggerWorldEvent")]
        [SerializeField]  private UnityEvent _worldEvent;

        private bool _hasFired;
        private EventBinding<OnTutorialCompleted> _tutorialCompletedEvent;

        private void OnEnable()
        {
            _tutorialCompletedEvent = new EventBinding<OnTutorialCompleted>(DestroyTrigger);
            ServiceLocator.ForSceneOf(this).Get<EventBusRegistry>().Get<UIEventBus>().Register(_tutorialCompletedEvent);
        }

        private void OnDisable()
        {
            ServiceLocator.ForSceneOf(this).Get<EventBusRegistry>().Get<UIEventBus>().Deregister(_tutorialCompletedEvent);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerController>())
            {
                Fire();
            }
        }

        private void Fire()
        {
            if (fireOnce && _hasFired) return;
            if (triggerWorldEvent && _worldEvent != null) _worldEvent.Invoke();

            _hasFired = true;
            ServiceLocator.ForSceneOf(this).Get<EventBusRegistry>().Get<UIEventBus>().Raise(new OnRequestTutorialEvent { Sequence = sequence });
        }

        private void DestroyTrigger(OnTutorialCompleted @event)
        {
            //if (@event.TutorialId == sequence.tutorialId) Destroy(gameObject);
        }
    }
}