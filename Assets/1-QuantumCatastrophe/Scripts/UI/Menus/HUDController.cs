// HUDController.cs
//
// Owns the health orb row on HUDDocument (sort order 0). Purely reactive — listens for
// OnHealthChangedEvent (published by your health code) and reflects current/max health
// as filled/empty orbs. Assumes a fixed row of 5 orbs named "orb-0".."orb-4" in the UXML.

using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;
using UnityEngine;
using UnityEngine.UIElements;

namespace QC.Systems.HUD
{
    public class HUDController : MonoBehaviour
    {
        [Header("Document refs")]
        [SerializeField] private UIDocument hudDocument;

        [Header("Health orbs")]
        [SerializeField] private int orbCount = 5;

        private VisualElement _root;
        private VisualElement[] _orbs;
        private int _lastHealth = -1;

        private UIEventBus _uiEventBus;
        private EventBinding<OnHealthChangedEvent> _onHealthChangedEvent;

        private void Awake()
        {
            _uiEventBus = ServiceLocator.ForSceneOf(this).Get<EventBusRegistry>().Get<UIEventBus>();
            _onHealthChangedEvent = new EventBinding<OnHealthChangedEvent>(OnHealthChanged);
        }

        private void OnEnable()
        {
            _root = hudDocument.rootVisualElement;

            _orbs = new VisualElement[orbCount];
            for (int i = 0; i < orbCount; i++)
            {
                _orbs[i] = _root.Q<VisualElement>($"orb-{i}");
            }

            
            _uiEventBus.Register(_onHealthChangedEvent);
        }

        private void OnDisable()
        {
            _uiEventBus.Deregister(_onHealthChangedEvent);
        }

        private void OnHealthChanged(OnHealthChangedEvent evt)
        {
            int clampedCurrent = Mathf.Clamp(evt.Current, 0, orbCount);

            for (int i = 0; i < _orbs.Length; i++)
            {
                bool filled = i < clampedCurrent;
                _orbs[i].EnableInClassList("orb-filled", filled);
            }

            // Punch the orb that was just lost, if health dropped
            if (_lastHealth > clampedCurrent && clampedCurrent < _orbs.Length)
            {
                VisualElement lostOrb = _orbs[clampedCurrent];
                lostOrb.AddToClassList("orb-just-lost");
                lostOrb.schedule.Execute(() => lostOrb.RemoveFromClassList("orb-just-lost")).StartingIn(150);
            }

            _lastHealth = clampedCurrent;
        }
    }
}
