using UnityEngine;

namespace QC.Systems.Entanglement.Strategies
{
    /// <summary>
    /// Propagates linear velocity from the source's Rigidbody2D to the target's Rigidbody2D.
    /// </summary>
    [System.Serializable]
    public class VelocityEntanglementStrategy : IEntanglementStrategy
    {
        [Tooltip("How strongly the source velocity is applied to the target. 1 = full copy.")]
        [Range(0f, 1f)]
        public float Strength = 1f;

        public void OnEntangled(EntanglableComponent source, EntanglableComponent target) { }

        public void OnDisentangled(EntanglableComponent source, EntanglableComponent target) { }

        public void Apply(EntanglableComponent source, EntanglableComponent target, float impedanceFactor)
        {
            Rigidbody2D sourceRb = source.GetComponent<Rigidbody2D>();
            Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();

            if (sourceRb == null || targetRb == null) return;

#if UNITY_6000_0_OR_NEWER
            Vector2 sourceVelocity = sourceRb.linearVelocity;
#else
            Vector2 sourceVelocity = sourceRb.velocity;
#endif

            Vector2 propagated = sourceVelocity * Strength * (1f - impedanceFactor);

#if UNITY_6000_0_OR_NEWER
            targetRb.linearVelocity = propagated;
#else
            targetRb.velocity = propagated;
#endif
        }
    }
}