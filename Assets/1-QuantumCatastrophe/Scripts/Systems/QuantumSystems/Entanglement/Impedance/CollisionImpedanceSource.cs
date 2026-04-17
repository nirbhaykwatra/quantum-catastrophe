using UnityEngine;

namespace QC.Systems.Entanglement.Impedance
{

    /// <summary>
    /// Samples impedance by casting a box in all four cardinal directions and
    /// computing how boxed-in the object is based on proximity of surrounding geometry.
    /// </summary>
    [System.Serializable]
    public class CollisionImpedanceSource : IImpedanceSource
    {
        [Tooltip("Maximum detection range per direction. At or beyond this distance, impedance contribution is 0.")]
        public float DetectionRange = 1f;

        [Tooltip("Size of the overlap box cast in each direction.")]
        public float CastRadius = 0.2f;

        [Tooltip("Layer mask for geometry that counts as an obstacle.")]
        public LayerMask ObstacleMask;

        // Cached reference set by EntanglableComponent at entangle time (set via Init).
        private Transform m_transform;

        public void Init(Transform t) => m_transform = t;

        public float GetImpedance()
        {
            if (m_transform == null) return 0f;

            Vector3 pos = m_transform.position;
            float totalBlocking = 0f;
            int directionCount = 0;

            // Check four horizontal directions and up/down.
            Vector3[] directions =
            {
                Vector3.right, Vector3.left,
                Vector3.up,    Vector3.down,
                Vector3.forward, Vector3.back
            };

            foreach (Vector3 dir in directions)
            {
                directionCount++;
                RaycastHit hit;
                if (Physics.SphereCast(pos, CastRadius, dir, out hit, DetectionRange, ObstacleMask))
                {
                    // Closer hit = higher impedance contribution for this direction.
                    float proximity = 1f - Mathf.Clamp01(hit.distance / DetectionRange);
                    totalBlocking += proximity;
                }
            }

            return Mathf.Clamp01(totalBlocking / directionCount);
        }
    }
}