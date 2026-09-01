using System.Collections.Generic;
using QC.Character;
using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace QC.Systems.Entanglement
{
    /// <summary>
    /// Handles the player-facing entanglement mode: toggling the cursor,
    /// highlighting nearby EntanglableComponents, and managing the selection
    /// flow that forms or breaks pairs.
    ///
    /// Entangle:     click a free object or Source → click another free object.
    /// Disentangle:  click an entangled Target directly (single click).
    /// </summary>
    [RequireComponent(typeof(CharacterAbilities))]
    public class EntanglementModeController : MonoBehaviour
    {
        // ── Selection state machine ───────────────────────────────────────────

        private enum SelectionState
        {
            Inactive,
            AwaitingFirstSelection,
            AwaitingSecondSelection
        }

        // ── Inspector fields ──────────────────────────────────────────────────

        [Tooltip("World-space radius around the player in which objects can be highlighted.")]
        [SerializeField]
        private float m_selectionRadius = 8f;

        [Tooltip("Layer mask for EntanglableComponent objects.")]
        [SerializeField]
        private LayerMask m_entanglableMask;

        [Tooltip("Camera used for screen-to-world raycasting when clicking.")]
        [SerializeField]
        private Camera m_camera;

        // ── Runtime state ─────────────────────────────────────────────────────

        private CharacterAbilities m_abilities;
        private GlobalEventBus m_globalEventBus;
        private EntanglementManager m_entanglementManager;
        private SelectionState m_state = SelectionState.Inactive;

        private EntanglableComponent m_firstSelection;

        private readonly List<EntanglableComponent> m_currentHighlighted = new();

        private CursorLockMode m_previousCursorLockMode;
        private bool m_previousCursorVisible;

        // Event bindings
        private EventBinding<OnToggleEntanglement> m_toggleEntanglementBinding;
        private EventBinding<OnClickEntangle> m_clickEntangleBinding;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            m_abilities = GetComponent<CharacterAbilities>();
            if (m_camera == null) m_camera = Camera.main;
            m_globalEventBus = ServiceLocator.Global.Get<EventBusRegistry>().Get<GlobalEventBus>();
        }

        private void Start()
        {
            m_entanglementManager = ServiceLocator.ForSceneOf(this).Get<EntanglementManager>();
        }

        private void OnEnable()
        {
            m_toggleEntanglementBinding = new EventBinding<OnToggleEntanglement>(OnToggleEntanglementMode);
            m_clickEntangleBinding = new EventBinding<OnClickEntangle>(OnEntanglementSelect);

            m_globalEventBus.Register(m_toggleEntanglementBinding);
            m_globalEventBus.Register(m_clickEntangleBinding);
        }

        private void OnDisable()
        {
            m_globalEventBus.Deregister(m_toggleEntanglementBinding);
            m_globalEventBus.Deregister(m_clickEntangleBinding);
        }

        private void Update()
        {
            if (m_state == SelectionState.Inactive) return;
            RefreshHighlights();
        }

        // ── Input callbacks ───────────────────────────────────────────────────

        public void OnToggleEntanglementMode(OnToggleEntanglement @event)
        {
            if (!m_abilities.EnableEntanglementMode) return;
            
            if (m_state == SelectionState.Inactive)
                EnterEntanglementMode();
            else
                ExitEntanglementMode();
        }

        public void OnEntanglementSelect(@OnClickEntangle @event)
        {
            if (m_state == SelectionState.Inactive) return;

            EntanglableComponent clicked = RaycastForEntanglable();

            switch (m_state)
            {
                case SelectionState.AwaitingFirstSelection:
                    HandleFirstSelection(clicked);
                    break;
                case SelectionState.AwaitingSecondSelection:
                    HandleSecondSelection(clicked);
                    break;
            }
        }

        // ── Mode transition ───────────────────────────────────────────────────

        private void EnterEntanglementMode()
        {
            m_state = SelectionState.AwaitingFirstSelection;
            m_firstSelection = null;

            m_previousCursorLockMode = Cursor.lockState;
            m_previousCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            m_globalEventBus.Raise(new OnModeChanged { Mode = PlayerMode.Entangle });
        }

        private void ExitEntanglementMode()
        {
            if (m_firstSelection != null)
            {
                m_firstSelection.ChangeHighlightColor(Color.red);
                m_firstSelection.Unhighlight();
                m_firstSelection = null;
            }

            ClearHighlights();
            m_state = SelectionState.Inactive;

            Cursor.lockState = m_previousCursorLockMode;
            Cursor.visible = m_previousCursorVisible;

            m_globalEventBus.Raise(new OnModeChanged { Mode = PlayerMode.Normal });
        }

        // ── Selection flow ────────────────────────────────────────────────────

        private void HandleFirstSelection(EntanglableComponent clicked)
        {
            if (clicked == null) return;

            // Clicking an entangled Target directly disentangles it.
            if (clicked.IsEntangled && clicked.Role == EntanglementRole.Target)
            {
                m_entanglementManager.BreakSpecificPair(clicked.Partner, clicked);
                ExitEntanglementMode();
                return;
            }

            // Free objects or Sources proceed to second-selection.
            m_firstSelection = clicked;
            clicked.ChangeHighlightColor(Color.green);
            m_state = SelectionState.AwaitingSecondSelection;
        }

        private void HandleSecondSelection(EntanglableComponent clicked)
        {
            if (clicked == null) return;
            if (clicked == m_firstSelection) return;

            // Source selected first, then one of its own existing targets → disentangle that pair.
            if (m_firstSelection.Role == EntanglementRole.Source &&
                clicked.Role == EntanglementRole.Target &&
                clicked.Partner == m_firstSelection)
            {
                m_entanglementManager.BreakSpecificPair(m_firstSelection, clicked);
                ExitEntanglementMode();
                return;
            }
            
            // If the clicked target is already entangled with a different source, break that pair.
            if (clicked.Role == EntanglementRole.Target && clicked.Partner != m_firstSelection)
            {
                m_entanglementManager.BreakSpecificPair(clicked.Partner, clicked);
                ExitEntanglementMode();
            }

            // All other cases → form a new pair (adds a target to a source, or links two free objects).
            m_globalEventBus.Raise(new OnEntanglementPairFormed
            {
                Source = m_firstSelection,
                Target = clicked
            });

            ExitEntanglementMode();
        }

        // ── Highlight management ──────────────────────────────────────────────

        private void RefreshHighlights()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, m_selectionRadius, m_entanglableMask);

            HashSet<EntanglableComponent> inRange = new();
            foreach (Collider2D hit in hits)
            {
                if (!hit.TryGetComponent(out EntanglableComponent ec)) continue;

                // Free objects can be entangled.
                bool isFree = !ec.IsEntangled;
                // Sources are always shown so the player can add more targets to them.
                bool isSource = ec.IsEntangled && ec.Role == EntanglementRole.Source;
                // Targets are always shown so the player can click them to disentangle.
                bool isTarget = ec.IsEntangled && ec.Role == EntanglementRole.Target;

                if (isFree || isSource || isTarget)
                    inRange.Add(ec);
            }

            // Remove highlights for objects no longer in the valid set.
            for (int i = m_currentHighlighted.Count - 1; i >= 0; i--)
            {
                EntanglableComponent ec = m_currentHighlighted[i];
                if (!inRange.Contains(ec))
                {
                    ec.Unhighlight();
                    m_currentHighlighted.RemoveAt(i);
                }
            }

            // Add highlights for newly valid objects.
            foreach (EntanglableComponent ec in inRange)
            {
                if (!m_currentHighlighted.Contains(ec))
                {
                    ec.HighlightAsEntangleable();
                    m_currentHighlighted.Add(ec);
                }
            }
        }

        private void ClearHighlights()
        {
            foreach (EntanglableComponent ec in m_currentHighlighted)
                ec.Unhighlight();
            m_currentHighlighted.Clear();
        }

        // ── Raycasting ────────────────────────────────────────────────────────

        private EntanglableComponent RaycastForEntanglable()
        {
            Ray ray = m_camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, m_entanglableMask);

            if (hit.collider != null && hit.collider.TryGetComponent(out EntanglableComponent ec))
                return ec;

            return null;
        }

        // ── Gizmos ────────────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, m_selectionRadius);
        }
    }
}