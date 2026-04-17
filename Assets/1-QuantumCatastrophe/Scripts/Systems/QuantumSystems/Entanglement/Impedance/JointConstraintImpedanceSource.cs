using UnityEngine;

namespace QC.Systems.Entanglement.Impedance
{
    /// <summary>
    /// Samples impedance from the reaction forces reported by an attached Joint.
    /// Works with HingeJoint, ConfigurableJoint, etc. (3D only).
    /// </summary>
    [System.Serializable]
    public class JointConstraintImpedanceSource : IImpedanceSource
    {
        [Tooltip("The joint whose constraint forces are read.")]
        public Joint Joint;

        [Tooltip("Force magnitude that maps to an impedance of 1.")]
        public float MaxForce = 100f;

        public float GetImpedance()
        {
            if (Joint == null) return 0f;
            float forceMagnitude = Joint.currentForce.magnitude;
            return Mathf.Clamp01(forceMagnitude / MaxForce);
        }
    }
}