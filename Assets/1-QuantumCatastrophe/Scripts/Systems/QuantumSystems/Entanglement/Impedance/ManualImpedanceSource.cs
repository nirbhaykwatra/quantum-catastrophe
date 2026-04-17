using UnityEngine;

namespace QC.Systems.Entanglement.Impedance
{
    /// <summary>
    /// A static, designer-set impedance value. Useful for scripted scenarios
    /// where an object should always resist receiving entanglement effects.
    /// </summary>
    [System.Serializable]
    public class ManualImpedanceSource : IImpedanceSource
    {
        [Tooltip("Fixed impedance value in [0..1].")]
        [Range(0f, 1f)]
        public float ImpedanceValue = 0f;

        public float GetImpedance() => ImpedanceValue;
    }
}