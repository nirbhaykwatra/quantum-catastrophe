using System;
using System.Collections.Generic;
using QC.Utilities.ServiceLocation;
using UnityEngine;

namespace QC.Systems.Entanglement
{
    /// <summary>
    /// The role this object plays within an entanglement pair.
    /// </summary>
    public enum EntanglementRole
    {
        None,
        /// <summary>First selected — the object whose attributes are read and propagated.</summary>
        Source,
        /// <summary>Second selected — the object that receives propagated attributes.</summary>
        Target
    }

    /// <summary>
    /// Attach to any GameObject that can participate in entanglement.
    /// Holds the lists of strategies and impedance sources configured by designers,
    /// and serves as the per-object façade for the entanglement system.
    /// </summary>
    public class EntanglableComponent : MonoBehaviour
    {
        // ── Designer-configured lists ─────────────────────────────────────────

        [Tooltip("Strategies that define which attributes this object propagates when it is the Source.")]
        [SerializeReference]
        private List<IEntanglementStrategy> m_strategies = new();

        [Tooltip("Sources of physical impedance on this object. Combined via Max().")]
        [SerializeReference]
        private List<IImpedanceSource> m_impedanceSources = new();
        
        [Tooltip("When enabled, the target's state is also propagated back to this source using the same strategies. " +
                 "The source's own impedance is used as the blocking factor for the reverse pass.")]
        [SerializeField]
        private bool m_propagateBackToSource = false;

        // ── Optional highlight visuals ────────────────────────────────────────

        [Tooltip("GameObject shown/hidden to indicate this object can be entangled.")]
        [SerializeField]
        private GameObject m_highlightObject;

        // ── Runtime state ─────────────────────────────────────────────────────

        public bool IsEntangled { get; private set; }
        public EntanglementRole Role { get; private set; } = EntanglementRole.None;

        // A Source may link to many Targets; a Target has exactly one Source.
        private readonly List<EntanglableComponent> m_partners = new();
        public IReadOnlyList<EntanglableComponent> Partners => m_partners;

        // Convenience: returns the single partner for a Target (its Source), or null.
        public EntanglableComponent Partner => m_partners.Count > 0 ? m_partners[0] : null;

        // ── Strategy accessors (used by EntanglementManager) ──────────────────

        public IReadOnlyList<IEntanglementStrategy> Strategies => m_strategies;
        public bool PropagateBackToSource => m_propagateBackToSource;

        private EntanglementManager m_entanglementManager;
        
        private void Start()
        {
            m_entanglementManager = ServiceLocator.ForSceneOf(this).Get<EntanglementManager>();
        }

        // ── Highlight API ─────────────────────────────────────────────────────

        /// <summary>Shows the highlight to indicate this object can be entangled.</summary>
        public void HighlightAsEntangleable()
        {
            if (m_highlightObject != null)
            {
                m_highlightObject.SetActive(true);
                SpriteRenderer spriteRenderer = m_highlightObject.GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null)
                    spriteRenderer.color = Color.red;
            }
        }

        /// <summary>Hides the highlight.</summary>
        public void Unhighlight()
        {
            if (m_highlightObject != null)
                m_highlightObject.SetActive(false);
        }

        // ── Entanglement lifecycle ────────────────────────────────────────────

        /// <summary>
        /// Adds a new partner to this component. Called by the EntanglementManager
        /// when this object is added to a pair.
        /// </summary>
        public void OnEntangled(EntanglableComponent partner, EntanglementRole role)
        {
            IsEntangled = true;
            Role = role;
            if (!m_partners.Contains(partner))
                m_partners.Add(partner);
            Unhighlight();
        }

        /// <summary>
        /// Removes a specific partner. Called by the EntanglementManager when a
        /// single pair is broken. Resets entanglement state only when no partners remain.
        /// </summary>
        public void OnDisentangled(EntanglableComponent partner)
        {
            m_partners.Remove(partner);
            if (m_partners.Count == 0)
            {
                IsEntangled = false;
                Role = EntanglementRole.None;
            }
        }

        // ── Impedance sampling ────────────────────────────────────────────────

        /// <summary>
        /// Aggregates all attached IImpedanceSources using Max, returning a
        /// value in [0..1]. 0 means completely free, 1 means fully blocked.
        /// </summary>
        public float SampleImpedance()
        {
            if (m_impedanceSources == null || m_impedanceSources.Count == 0)
                return 0f;

            float max = 0f;
            foreach (IImpedanceSource source in m_impedanceSources)
            {
                if (source == null) continue;
                float value = Mathf.Clamp01(source.GetImpedance());
                if (value > max) max = value;
            }
            return max;
        }

        private void OnDestroy()
        {
            if (IsEntangled)
                m_entanglementManager?.BreakPairContaining(this);
        }
    }
}