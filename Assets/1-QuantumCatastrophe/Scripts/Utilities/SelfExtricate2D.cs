using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class SelfExtricate2D : MonoBehaviour
{
    [Tooltip("Layers considered solid — the object will push itself out of anything on these layers. " +
             "Exclude the player/dynamic-actor layer here; this is for escaping level geometry, not resolving normal standing contact.")]
    [SerializeField] private LayerMask m_solidLayers;

    [Tooltip("Minimum penetration depth before this counts as \"stuck\" rather than normal resting contact. " +
             "Box2D allows a small amount of overlap (a few mm) on stable stacked/standing contacts — this must be well above that or you'll fight the solver.")]
    [SerializeField] private float m_minPenetrationDepth = 0.1f;

    [Tooltip("Extra separation applied beyond the exact overlap depth, so it doesn't re-trigger next frame due to float precision.")]
    [SerializeField] private float m_skinWidth = 0.01f;

    [Tooltip("Max distance moved per FixedUpdate while extricating, to avoid snapping through geometry in one step if deeply embedded.")]
    [SerializeField] private float m_maxPushPerStep = 0.5f;

    private Rigidbody2D m_rb;
    private Collider2D m_collider;
    private readonly Collider2D[] m_overlapBuffer = new Collider2D[8];
    private ContactFilter2D m_filter;

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
        m_collider = GetComponent<Collider2D>();

        m_filter = new ContactFilter2D();
        m_filter.SetLayerMask(m_solidLayers);
        m_filter.useTriggers = false;
    }

    private void FixedUpdate()
    {
        int count = m_collider.Overlap(m_filter, m_overlapBuffer);
        if (count == 0) return;

        Vector2 totalPush = Vector2.zero;

        for (int i = 0; i < count; i++)
        {
            Collider2D other = m_overlapBuffer[i];
            if (other == null || other == m_collider) continue;

            ColliderDistance2D dist = m_collider.Distance(other);
            if (!dist.isValid || !dist.isOverlapped) continue;

            float penetrationDepth = -dist.distance;

            // Ignore shallow overlap — that's normal resting/stacking contact, not being "stuck".
            if (penetrationDepth < m_minPenetrationDepth) continue;

            totalPush += dist.normal * (penetrationDepth + m_skinWidth);
        }

        if (totalPush.sqrMagnitude > 0f)
        {
            if (totalPush.magnitude > m_maxPushPerStep)
                totalPush = totalPush.normalized * m_maxPushPerStep;

            m_rb.MovePosition(m_rb.position + totalPush);
        }
    }
}