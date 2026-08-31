using System.Collections.Generic;
using UnityEngine;

namespace QC.Systems.Entanglement.Strategies
{
    /// <summary>
    /// Rotates the target by the same DELTA the source has rotated since the pair formed,
    /// rather than blending the target toward the source's absolute rotation. This means the
    /// target keeps whatever rotation it had at pairing time and simply accumulates the same
    /// spin the source has accumulated since then, instead of snapping toward the source's angle.
    /// </summary>
    [System.Serializable]
    public class RotationEntanglementStrategy : IEntanglementStrategy
    {
        [Tooltip("How strongly the computed delta rotation is applied to the target. 1 = full delta.")]
        [Range(0f, 1f)]
        public float Strength = 1f;

        // Anchors are keyed by an UNORDERED pair (via instance ID ordering) rather than by
        // (source, target) role, because EntanglementManager's back-propagation reverse pass
        // calls Apply with source/target swapped, but OnEntangled is only ever raised once in
        // the forward direction. Storing both components' anchor rotations under one canonical
        // key lets both the forward and reverse Apply calls resolve to the same anchor data.
        private readonly Dictionary<(EntanglableComponent A, EntanglableComponent B), (float RotA, float RotB)> m_anchors = new();

        private static (EntanglableComponent A, EntanglableComponent B) NormalizedKey(EntanglableComponent x, EntanglableComponent y)
        {
            return x.GetInstanceID() <= y.GetInstanceID() ? (x, y) : (y, x);
        }

        public void OnEntangled(EntanglableComponent source, EntanglableComponent target)
        {
            (EntanglableComponent A, EntanglableComponent B) key = NormalizedKey(source, target);
            float sourceRot = source.transform.localEulerAngles.z;
            float targetRot = target.transform.localEulerAngles.z;

            m_anchors[key] = key.A == source ? (sourceRot, targetRot) : (targetRot, sourceRot);
        }

        public void OnDisentangled(EntanglableComponent source, EntanglableComponent target)
        {
            m_anchors.Remove(NormalizedKey(source, target));
        }

        public void Apply(EntanglableComponent source, EntanglableComponent target, float impedanceFactor)
        {
            if (source == null || target == null) return;

            var key = NormalizedKey(source, target);
            if (!m_anchors.TryGetValue(key, out var anchor)) return;

            float sourceAnchorRot = key.A == source ? anchor.RotA : anchor.RotB;
            float targetAnchorRot = key.A == source ? anchor.RotB : anchor.RotA;

            float currentSourceRot = source.transform.localEulerAngles.z;
            float delta = Mathf.DeltaAngle(sourceAnchorRot, currentSourceRot);

            float desiredTargetRot = targetAnchorRot + delta;
            float currentTargetRot = target.transform.localEulerAngles.z;

            float blendFactor = Strength * (1f - impedanceFactor);
            float resultZ = Mathf.LerpAngle(currentTargetRot, desiredTargetRot, blendFactor);

            target.transform.localRotation = Quaternion.Euler(0f, 0f, resultZ);
        }
    }
}