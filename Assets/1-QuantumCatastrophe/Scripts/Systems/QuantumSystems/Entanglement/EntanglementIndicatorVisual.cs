using UnityEngine;
using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;

namespace QC.Systems.Entanglement
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(EntanglableComponent))]
    public class EntanglementIndicatorVisual : MonoBehaviour
    {
        [Tooltip("How fast the glow fades in/out, in activation-units per second (1 = full fade in 1s).")]
        [SerializeField] private float m_fadeSpeed = 4f;

        private static readonly int s_activationId = Shader.PropertyToID("_ActivationT");

        private SpriteRenderer m_renderer;
        private EntanglableComponent m_entanglable;
        private MaterialPropertyBlock m_propBlock;
        private float m_currentActivation;
        private float m_targetActivation;

        private GlobalEventBus m_globalEventBus;
        private EventBinding<OnEntanglementPairFormed> m_onPairFormed;
        private EventBinding<OnEntanglementPairBroken> m_onPairBroken;

        private void Awake()
        {
            m_renderer = GetComponent<SpriteRenderer>();
            m_entanglable = GetComponent<EntanglableComponent>();
            m_propBlock = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            m_globalEventBus = ServiceLocator.Global.Get<EventBusRegistry>().Get<GlobalEventBus>();
            m_onPairFormed = new EventBinding<OnEntanglementPairFormed>(HandlePairFormed);
            m_onPairBroken = new EventBinding<OnEntanglementPairBroken>(HandlePairBroken);
            m_globalEventBus.Register(m_onPairFormed);
            m_globalEventBus.Register(m_onPairBroken);

            // Snap (don't fade) to the correct state if this is already entangled when enabled.
            m_targetActivation = m_entanglable.IsEntangled ? 1f : 0f;
            m_currentActivation = m_targetActivation;
            ApplyActivation(m_currentActivation);
        }

        private void OnDisable()
        {
            m_globalEventBus.Deregister(m_onPairFormed);
            m_globalEventBus.Deregister(m_onPairBroken);
        }

        private void Update()
        {
            if (Mathf.Approximately(m_currentActivation, m_targetActivation)) return;
            m_currentActivation = Mathf.MoveTowards(m_currentActivation, m_targetActivation, m_fadeSpeed * Time.deltaTime);
            ApplyActivation(m_currentActivation);
        }

        private void HandlePairFormed(OnEntanglementPairFormed evt)
        {
            if (evt.Target == m_entanglable)
                m_targetActivation = 1f;
        }

        private void HandlePairBroken(OnEntanglementPairBroken evt)
        {
            // Targets can only belong to one pair at a time (per your manager's comment),
            // so IsEntangled here just confirms nothing re-paired it in the same frame.
            if (evt.Target == m_entanglable && !m_entanglable.IsEntangled)
                m_targetActivation = 0f;
        }

        private void ApplyActivation(float value)
        {
            m_renderer.GetPropertyBlock(m_propBlock);
            m_propBlock.SetFloat(s_activationId, value);
            m_renderer.SetPropertyBlock(m_propBlock);
        }
    }
}