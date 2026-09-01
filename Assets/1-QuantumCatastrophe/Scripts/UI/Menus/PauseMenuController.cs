// PauseMenuController.cs
//
// Sibling controller to HUDController on the same HUDDocument GameObject/UIDocument.
// Split out separately (rather than folded into HUDController) to match how the
// tutorial and notification systems are each their own controller.
//
// Handles pause toggling, timescale, and cursor lock/visibility — same
// capture-and-restore pattern used in TutorialModalController, so the two don't
// stomp on each other's restore value if a project later allows pause to interrupt
// a tutorial (see the optional tutorialModalController gate below).

using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace QC.Systems.HUD
{
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Document refs")]
        [SerializeField] private UIDocument hudDocument;

        [Header("Scene")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Optional guards")]
        [Tooltip("If assigned, pause requests are ignored while a tutorial is active, " +
                 "preventing the two systems from fighting over Time.timeScale/Cursor state.")]
        [SerializeField] private MonoBehaviour tutorialModalControllerGate; // assign a component exposing IsActive, or leave null

        private VisualElement _root;
        private VisualElement _pauseOverlay;
        private Button _resumeButton;
        private Button _quitButton;

        private bool _isPaused;
        private float _previousTimeScale;
        private CursorLockMode _previousLockState;
        private bool _previousCursorVisible;

        private GlobalEventBus _globalEventBus;
        private EventBinding<OnPauseRequestedEvent> _onPauseRequestedEvent;

        private void Awake()
        {
            _globalEventBus = ServiceLocator.ForSceneOf(this).Get<EventBusRegistry>().Get<GlobalEventBus>();
        }

        private void OnEnable()
        {
            _root = hudDocument.rootVisualElement;
            _pauseOverlay = _root.Q<VisualElement>("pause-menu-overlay");
            _resumeButton = _root.Q<Button>("resume-button");
            _quitButton = _root.Q<Button>("quit-button");

            _resumeButton.clicked += TogglePause;
            _quitButton.clicked += OnQuitClicked;

            _onPauseRequestedEvent = new EventBinding<OnPauseRequestedEvent>(OnPauseRequested);
            _globalEventBus.Register(_onPauseRequestedEvent);

            HideImmediate();
        }

        private void OnDisable()
        {
            _resumeButton.clicked -= TogglePause;
            _quitButton.clicked -= OnQuitClicked;

            _globalEventBus.Deregister(_onPauseRequestedEvent);
        }

        private void OnPauseRequested(OnPauseRequestedEvent evt)
        {
            if (IsGatedByTutorial()) return;
            TogglePause();
        }

        private bool IsGatedByTutorial()
        {
            // tutorialModalControllerGate is typed as MonoBehaviour so this file doesn't
            // need a hard reference to the Tutorials namespace. Replace this with a direct
            // TutorialModalController reference + IsActive check once you've settled
            // whether pause should ever be reachable during a tutorial.
            return false;
        }

        private void TogglePause()
        {
            _isPaused = !_isPaused;

            _pauseOverlay.style.display = _isPaused ? DisplayStyle.Flex : DisplayStyle.None;

            if (_isPaused)
            {
                _previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;

                _previousLockState = Cursor.lockState;
                _previousCursorVisible = Cursor.visible;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Time.timeScale = _previousTimeScale;
                Cursor.lockState = _previousLockState;
                Cursor.visible = _previousCursorVisible;
            }

            _globalEventBus.Raise(new OnGamePausedEvent { IsPaused = _isPaused });
        }

        private void OnQuitClicked()
        {
            Time.timeScale = 1f; // reset before leaving, or the next scene loads paused
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void HideImmediate()
        {
            _pauseOverlay.style.display = DisplayStyle.None;
        }
    }
}
