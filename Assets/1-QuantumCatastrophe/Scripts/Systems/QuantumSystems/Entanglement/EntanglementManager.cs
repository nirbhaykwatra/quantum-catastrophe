using System;
using System.Collections.Generic;
using QC.Utilities;
using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;
using UnityEngine;

namespace QC.Systems.Entanglement
{
    /// <summary>
    /// Scene-level service that maintains all active entanglement pairs and
    /// orchestrates the per-FixedUpdate propagation loop.
    /// </summary>
    public class EntanglementManager : PersistentSingleton<EntanglementManager>
    {
        // ── Internal pair record ──────────────────────────────────────────────

        private readonly struct EntanglementPair
        {
            public readonly EntanglableComponent Source;
            public readonly EntanglableComponent Target;

            public EntanglementPair(EntanglableComponent source, EntanglableComponent target)
            {
                Source = source;
                Target = target;
            }
        }

        private readonly List<EntanglementPair> m_pairs = new();

        // ── Link visuals ──────────────────────────────────────────────────────

        [Tooltip("Prefab with a LineRenderer + LinkBeamVisual component. Instantiated per active pair.")]
        [SerializeField]
        private LinkBeamVisual m_linkVisualPrefab;

        private readonly Dictionary<(EntanglableComponent Source, EntanglableComponent Target), LinkBeamVisual> m_activeVisuals = new();

        // ── Back-propagation damping ──────────────────────────────────────────

        [Tooltip("When the target is blocked, this curve maps impedanceFactor to a drag multiplier " +
                 "applied to the source's Rigidbody2D, slowing it proportionally.")]
        [SerializeField]
        private AnimationCurve m_backPropDampingCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Tooltip("Maximum additional linear drag applied to the source during back-propagation.")]
        [SerializeField]
        private float m_maxBackPropDrag = 10f;

        // ── Event bus ─────────────────────────────────────────────────────────

        private GlobalEventBus m_globalEventBus;
        private EventBinding<OnEntanglementPairFormed> m_onPairFormed;
        private EventBinding<OnEntanglementPairBroken> m_onPairBroken;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        protected override void Awake()
        {
            base.Awake();
            ServiceLocator.ForSceneOf(this).Register(this);
        }

        private void OnEnable()
        {
            m_globalEventBus = ServiceLocator.Global.Get<EventBusRegistry>().Get<GlobalEventBus>();
            m_onPairFormed = new EventBinding<OnEntanglementPairFormed>(HandlePairFormed);
            m_onPairBroken = new EventBinding<OnEntanglementPairBroken>(HandlePairBroken);
            m_globalEventBus.Register(m_onPairFormed);
            m_globalEventBus.Register(m_onPairBroken);
        }

        private void OnDisable()
        {
            m_globalEventBus.Deregister(m_onPairFormed);
            m_globalEventBus.Deregister(m_onPairBroken);
        }

        // ── Per-frame propagation ─────────────────────────────────────────────

        private void FixedUpdate()
        {
            // Iterate backwards so we can safely remove stale pairs.
            for (int i = m_pairs.Count - 1; i >= 0; i--)
            {
                EntanglementPair pair = m_pairs[i];

                if (pair.Source == null || pair.Target == null)
                {
                    RemovePairAt(i);
                    continue;
                }

                float targetImpedance = pair.Target.SampleImpedance();

                foreach (IEntanglementStrategy strategy in pair.Source.Strategies)
                    strategy.Apply(pair.Source, pair.Target, targetImpedance);

                ApplyBackPropagationDamping(pair.Source, targetImpedance);
                
                // Reverse pass: propagate the target's state back to the source.
                if (pair.Source.PropagateBackToSource)
                {
                    float sourceImpedance = pair.Source.SampleImpedance();
                    foreach (IEntanglementStrategy strategy in pair.Source.Strategies)
                        strategy.Apply(pair.Target, pair.Source, sourceImpedance);
                }
            }
        }

        // ── Back-propagation ──────────────────────────────────────────────────

        private void ApplyBackPropagationDamping(EntanglableComponent source, float targetImpedance)
        {
            Rigidbody2D rb = source.GetComponent<Rigidbody2D>();
            if (rb == null) return;

            float dampingMultiplier = m_backPropDampingCurve.Evaluate(targetImpedance);
            rb.linearDamping = dampingMultiplier * m_maxBackPropDrag;
        }

        // ── Pair management ───────────────────────────────────────────────────

        private void HandlePairFormed(OnEntanglementPairFormed evt)
        {
            if (evt.Source == null || evt.Target == null) return;

            // A Target can only belong to one pair — break any existing pair it is in.
            // The Source keeps its existing pairs, enabling one-to-many entanglement.
            BreakPairContaining(evt.Target);

            EntanglementPair pair = new(evt.Source, evt.Target);
            m_pairs.Add(pair);

            evt.Source.OnEntangled(evt.Target, EntanglementRole.Source);
            evt.Target.OnEntangled(evt.Source, EntanglementRole.Target);

            foreach (IEntanglementStrategy strategy in evt.Source.Strategies)
                strategy.OnEntangled(evt.Source, evt.Target);

            SpawnLinkVisual(evt.Source, evt.Target);
        }

        // ── Link visual management ────────────────────────────────────────────

        private void SpawnLinkVisual(EntanglableComponent source, EntanglableComponent target)
        {
            if (m_linkVisualPrefab == null) return;

            var key = (source, target);
            if (m_activeVisuals.ContainsKey(key)) return; // already visualized, don't double-spawn

            LinkBeamVisual visual = Instantiate(m_linkVisualPrefab);
            visual.SetEndpoints(source.transform, target.transform);
            m_activeVisuals[key] = visual;
        }

        private void DespawnLinkVisual(EntanglableComponent source, EntanglableComponent target)
        {
            var key = (source, target);
            if (m_activeVisuals.TryGetValue(key, out LinkBeamVisual visual))
            {
                if (visual != null) Destroy(visual.gameObject);
                m_activeVisuals.Remove(key);
            }
        }

        private void HandlePairBroken(OnEntanglementPairBroken evt)
        {
            for (int i = m_pairs.Count - 1; i >= 0; i--)
            {
                EntanglementPair pair = m_pairs[i];
                if (pair.Source == evt.Source && pair.Target == evt.Target)
                {
                    RemovePairAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Breaks all pairs that contain the given component as either source or target.
        /// Used when an object is destroyed or a target is reassigned.
        /// </summary>
        public void BreakPairContaining(EntanglableComponent component)
        {
            for (int i = m_pairs.Count - 1; i >= 0; i--)
            {
                EntanglementPair pair = m_pairs[i];
                if (pair.Source == component || pair.Target == component)
                {
                    m_globalEventBus?.Raise(new OnEntanglementPairBroken
                    {
                        Source = pair.Source,
                        Target = pair.Target
                    });
                    RemovePairAt(i);
                }
            }
        }

        /// <summary>
        /// Breaks only the specific pair between a known source and target.
        /// Used by the controller for the targeted disentangle gesture.
        /// </summary>
        public void BreakSpecificPair(EntanglableComponent source, EntanglableComponent target)
        {
            for (int i = m_pairs.Count - 1; i >= 0; i--)
            {
                EntanglementPair pair = m_pairs[i];
                if (pair.Source == source && pair.Target == target)
                {
                    RemovePairAt(i);
                    m_globalEventBus?.Raise(new OnEntanglementPairBroken
                    {
                        Source = pair.Source,
                        Target = pair.Target
                    });
                    return;
                }
            }
        }

        private void RemovePairAt(int index)
        {
            EntanglementPair pair = m_pairs[index];
            m_pairs.RemoveAt(index);

            DespawnLinkVisual(pair.Source, pair.Target);

            // Notify each component, removing only the specific partner link.
            pair.Source?.OnDisentangled(pair.Target);
            pair.Target?.OnDisentangled(pair.Source);

            // Restore source drag only if it has no remaining pairs.
            if (pair.Source != null && !pair.Source.IsEntangled)
            {
                Rigidbody2D rb = pair.Source.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearDamping = 0f;
            }

            // Let strategies clean up for this specific pair.
            if (pair.Source != null)
            {
                foreach (IEntanglementStrategy strategy in pair.Source.Strategies)
                    strategy.OnDisentangled(pair.Source, pair.Target);
            }
        }
    }
}