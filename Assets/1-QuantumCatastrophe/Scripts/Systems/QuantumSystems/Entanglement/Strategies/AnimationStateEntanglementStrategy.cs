using UnityEngine;

namespace QC.Systems.Entanglement.Strategies
{
    /// <summary>
    /// Mirrors the source's Animator trigger/bool states to the target's Animator.
    /// </summary>
    [System.Serializable]
    public class AnimationStateEntanglementStrategy : IEntanglementStrategy
    {
        [Tooltip("The name of the Animator bool parameter to mirror.")]
        public string BoolParameterName;

        private Animator m_sourceAnimator;
        private Animator m_targetAnimator;
        private int m_paramHash;

        public void OnEntangled(EntanglableComponent source, EntanglableComponent target)
        {
            m_sourceAnimator = source.GetComponentInChildren<Animator>();
            m_targetAnimator = target.GetComponentInChildren<Animator>();
            m_paramHash = Animator.StringToHash(BoolParameterName);
        }

        public void OnDisentangled(EntanglableComponent source, EntanglableComponent target)
        {
            m_sourceAnimator = null;
            m_targetAnimator = null;
        }

        public void Apply(EntanglableComponent source, EntanglableComponent target, float impedanceFactor)
        {
            if (m_sourceAnimator == null || m_targetAnimator == null) return;

            Animator sourceAnimator = source.GetComponent<Animator>();
            Animator targetAnimator = target.GetComponent<Animator>();
            
            if (string.IsNullOrEmpty(BoolParameterName)) return;

            // Impedance doesn't attenuate a bool — if fully blocked (impedance == 1) we skip.
            if (impedanceFactor >= 1f) return;

            bool value = sourceAnimator.GetBool(m_paramHash);
            targetAnimator.SetBool(m_paramHash, value);
        }
    }
}