using System;
using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace QC.Systems.Superposition
{
    public class SuperpositionObject : MonoBehaviour
    {
        [SerializeField]
        private bool _observed = true;

        [SerializeField]
        private float _unobservedTransparency = 0.5f;

        private Collider2D _collider;
        private SpriteRenderer _spriteRenderer;
        
        private GlobalEventBus _eventBus;
        private EventBinding<OnToggleSuperposition> _superpositionBinding;

        private void Awake()
        {
            _eventBus = ServiceLocator.ForSceneOf(this).Get<EventBusRegistry>().Get<GlobalEventBus>();
            _collider = GetComponent<Collider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();

            _collider.enabled = _observed;
            Color c = _spriteRenderer.color;
            c.a = _observed ? 1f : _unobservedTransparency;
            _spriteRenderer.color = c;
        }

        private void OnEnable()
        {
            _superpositionBinding = new EventBinding<OnToggleSuperposition>(ToggleSuperposition);
            _eventBus.Register(_superpositionBinding);
        }

        private void OnDisable()
        {
            _eventBus.Deregister(_superpositionBinding);
        }

        private void ToggleSuperposition()
        {
            _observed = !_observed;
            _collider.enabled = _observed;
            Color c = _spriteRenderer.color;
            c.a = _observed ? 1f : _unobservedTransparency;
            _spriteRenderer.color = c;
        }
    }
}