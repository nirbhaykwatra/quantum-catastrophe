using UnityEngine;

namespace QC.Systems.Entanglement.Strategies
{
    /// <summary>
    /// Propagates angular velocity (spin) from the source's Rigidbody2D to the target's Rigidbody2D.
    /// </summary>
    [System.Serializable]
    public class RotationEntanglementStrategy : IEntanglementStrategy
    {
        [Tooltip("How strongly the source angular velocity is applied to the target. 1 = full copy.")]
        [Range(0f, 1f)]
        public float Strength = 1f;
        

        public void OnEntangled(EntanglableComponent source, EntanglableComponent target)
        {

        }

        public void OnDisentangled(EntanglableComponent source, EntanglableComponent target)
        {

        }

        public void Apply(EntanglableComponent source, EntanglableComponent target, float impedanceFactor)
        {
            if (source == null || target == null) return;
            
            Transform sourceTransform = source.transform;
            Transform targetTransform = target.transform;
            
            float sourceZ = sourceTransform.localEulerAngles.z;
            float targetZ = targetTransform.localEulerAngles.z;
            float blendFactor = Strength * (1f - impedanceFactor);
            float resultZ = Mathf.LerpAngle(targetZ, sourceZ, blendFactor);

            targetTransform.localRotation = Quaternion.Euler(0f, 0f, resultZ);
        }
    }
}