// TutorialModalController.cs
//
// Owns the TutorialModalDocument UIDocument (highest sort order, blocks input while active).
// Subscribes to RequestTutorialEvent to begin a sequence and GameplaySignalEvent to advance
// steps waiting on a specific in-game action. Supports highlighting either a UI Toolkit
// VisualElement or an in-world scene object (via TutorialTargetRegistry).
//
// Expects EventBus to be a MonoBehaviour singleton (EventBus.Instance) with
// Subscribe<T>(Action<T>) / Unsubscribe<T>(Action<T>) / Publish<T>(T) methods.
// Expects a SaveData static class with HasSeenTutorial(id) / MarkTutorialSeen(id).

using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace QC.Systems.Tutorials
{
    public class TutorialModalController : MonoBehaviour
    {
        [Header("Document refs")]
        [SerializeField] private UIDocument modalDocument;
        [SerializeField] private VisualTreeAsset highlightRectTemplate; // template with a "dim-rect" element

        private VisualElement _root;
        private VisualElement _modalPanel;
        private Label _headerLabel;
        private Label _bodyLabel;
        private VisualElement _illustration;
        private Button _nextButton;
        private VisualElement _highlightOverlay;

        private TutorialSequenceSO _currentSequence;
        private int _stepIndex;
        private bool _waitingForEvent;
        private string _pendingEventName;

        // World-highlight follow state
        private Camera _mainCamera;
        private string _activeWorldTargetId;
        private float _activeWorldRadius;
        private float _previousTimeScale;
        private CursorLockMode _previousLockState;
        private bool _previousCursorVisible;

        private UIEventBus _uiEventBus;

        private EventBinding<OnRequestTutorialEvent> _onRequestTutorialEvent;
        private EventBinding<OnGameplaySignalEvent> _onGameplaySignalEvent;
        private EventBinding<OnTutorialModalOpened> _onTutorialModalOpened;
        private EventBinding<OnTutorialModalClosed> _onTutorialModalClosed;
        private EventBinding<OnTutorialCompleted> _onTutorialCompleted;

        public bool IsActive { get; private set; }

        private void Awake()
        {
            _uiEventBus = ServiceLocator.ForSceneOf(this).Get<EventBusRegistry>().Get<UIEventBus>();
        }

        private void OnEnable()
        {
            _root = modalDocument.rootVisualElement;
            _modalPanel = _root.Q<VisualElement>("modal-panel");
            _headerLabel = _root.Q<Label>("header-label");
            _bodyLabel = _root.Q<Label>("body-label");
            _illustration = _root.Q<VisualElement>("illustration");
            _nextButton = _root.Q<Button>("next-button");
            _highlightOverlay = _root.Q<VisualElement>("highlight-overlay");

            _nextButton.clicked += OnNextClicked;
            HideImmediate();

            _onRequestTutorialEvent = new EventBinding<OnRequestTutorialEvent>(OnTutorialRequested);
            _onGameplaySignalEvent = new EventBinding<OnGameplaySignalEvent>(OnGameplaySignal);
            
            _uiEventBus.Register(_onRequestTutorialEvent);
            _uiEventBus.Register(_onGameplaySignalEvent);
        }

        private void OnDisable()
        {
            _nextButton.clicked -= OnNextClicked;

            _uiEventBus.Deregister(_onRequestTutorialEvent);
            _uiEventBus.Deregister(_onGameplaySignalEvent);
        }

        private void Update()
        {
            // Keep a world-space highlight glued to its target as camera/object moves.
            if (IsActive && !string.IsNullOrEmpty(_activeWorldTargetId))
            {
                RefreshWorldHighlight();
            }
        }

        // ---------- Sequence lifecycle ----------

        private void OnTutorialRequested(OnRequestTutorialEvent evt)
        {
            if (IsActive) return; // don't interrupt an in-progress tutorial

            _currentSequence = evt.Sequence;
            _stepIndex = 0;
            BeginSequence();
        }

        private void BeginSequence()
        {
            IsActive = true;
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            _previousLockState = Cursor.lockState;
            _previousCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            _modalPanel.style.display = DisplayStyle.Flex;
            _uiEventBus.Raise(new OnTutorialModalOpened());
            ShowCurrentStep();

            _modalPanel.schedule.Execute(() => _modalPanel.AddToClassList("modal-visible")).StartingIn(0);
        }

        private void ShowCurrentStep()
        {
            TutorialStep step = _currentSequence.steps[_stepIndex];

            _headerLabel.text = step.headerText;
            _bodyLabel.text = step.bodyText;
            _illustration.style.backgroundImage = step.illustration != null
                ? new StyleBackground(step.illustration)
                : StyleKeyword.Null;

            UpdateHighlight(step);

            _waitingForEvent = step.advanceMode == TutorialAdvanceMode.WaitForEvent;
            _pendingEventName = step.requiredEventName;

            _nextButton.style.display = step.advanceMode == TutorialAdvanceMode.ButtonClick
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            if (step.advanceMode == TutorialAdvanceMode.Timer)
            {
                _modalPanel.schedule.Execute(Advance).StartingIn((long)(step.timerSeconds * 1000));
            }
        }

        private void OnGameplaySignal(OnGameplaySignalEvent evt)
        {
            if (!IsActive || !_waitingForEvent) return;
            // Only the current step's expected signal advances the tutorial — an
            // out-of-order click on a different target's signal is silently ignored.
            if (evt.SignalName == _pendingEventName)
            {
                Advance();
            }
        }

        private void OnNextClicked()
        {
            Advance();
        }

        private void Advance()
        {
            _stepIndex++;
            if (_stepIndex >= _currentSequence.steps.Length)
            {
                EndSequence();
            }
            else
            {
                ShowCurrentStep();
            }
        }

        private void EndSequence()
        {
            string id = _currentSequence.tutorialId;

            _modalPanel.RemoveFromClassList("modal-visible");
            _modalPanel.RegisterCallback<TransitionEndEvent>(OnCloseTransitionEnd);
        }

        private void OnCloseTransitionEnd(TransitionEndEvent evt)
        {
            _modalPanel.UnregisterCallback<TransitionEndEvent>(OnCloseTransitionEnd);
            HideImmediate();
            IsActive = false;
            _activeWorldTargetId = null;
            
            Time.timeScale = _previousTimeScale;
            Cursor.lockState = _previousLockState;
            Cursor.visible = _previousCursorVisible;

            string finishedId = _currentSequence.tutorialId;
            _currentSequence = null;

            _uiEventBus.Raise(new OnTutorialModalClosed());
            _uiEventBus.Raise(new OnTutorialCompleted { TutorialId = finishedId });
        }

        private void HideImmediate()
        {
            _modalPanel.style.display = DisplayStyle.None;
            _highlightOverlay.style.display = DisplayStyle.None;
        }

        // ---------- Highlighting ----------

        private void UpdateHighlight(TutorialStep step)
        {
            _highlightOverlay.Clear();
            _activeWorldTargetId = null;

            if (step.highlightMode == HighlightMode.None)
            {
                _highlightOverlay.style.display = DisplayStyle.None;
                return;
            }

            _highlightOverlay.style.display = DisplayStyle.Flex;

            if (step.highlightMode == HighlightMode.UIElement)
            {
                VisualElement target = _root.Q<VisualElement>(step.highlightTargetId);
                if (target != null) BuildSpotlightRects(target.worldBound);
                return;
            }

            // WorldObject
            _activeWorldTargetId = step.highlightTargetId;
            _activeWorldRadius = step.worldHighlightRadius;
            if (_mainCamera == null) _mainCamera = Camera.main;
            RefreshWorldHighlight();
        }

        private void RefreshWorldHighlight()
        {
            if (!TutorialTargetRegistry.TryGet(_activeWorldTargetId, out var target))
            {
                _highlightOverlay.style.display = DisplayStyle.None;
                return;
            }

            Vector2 screenPoint = RuntimeUtilities.CameraToPanelPoint(_mainCamera, target.position, _root.panel);
            float r = _activeWorldRadius;
            Rect rect = new Rect(screenPoint.x - r, screenPoint.y - r, r * 2f, r * 2f);
            BuildSpotlightRects(rect);
        }

        private void BuildSpotlightRects(Rect targetBounds)
        {
            // Four dimming rectangles around the target (top, bottom, left, right),
            // rebuilt fresh each call rather than pooled — tutorials are infrequent
            // enough that the allocation cost doesn't matter the way it would for toasts.
            _highlightOverlay.Clear();
            Rect screenRect = _root.worldBound;

            AddDimRect(0, 0, screenRect.width, targetBounds.y); // top
            AddDimRect(0, targetBounds.yMax, screenRect.width, screenRect.height - targetBounds.yMax); // bottom
            AddDimRect(0, targetBounds.y, targetBounds.x, targetBounds.height); // left
            AddDimRect(targetBounds.xMax, targetBounds.y, screenRect.width - targetBounds.xMax, targetBounds.height); // right
        }

        private void AddDimRect(float x, float y, float width, float height)
        {
            TemplateContainer rect = highlightRectTemplate.Instantiate();
            VisualElement el = rect.Q<VisualElement>("dim-rect");
            el.style.position = Position.Absolute;
            el.style.left = x;
            el.style.top = y;
            el.style.width = width;
            el.style.height = height;
            _highlightOverlay.Add(rect);
        }
    }
}


