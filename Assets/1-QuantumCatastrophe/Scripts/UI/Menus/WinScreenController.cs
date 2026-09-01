using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace QC.Systems.HUD
{
    public class WinScreenController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private ScreenFader screenFader;
    
        [Header("Scene")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private VisualElement winScreen;
        private Button retryButton;
        private Button menuButton;

        private UIEventBus _uiEventBus;
        private EventBinding<OnWin> _onWin;

        private void Awake()
        {
            VisualElement root = uiDocument.rootVisualElement;
            winScreen = root.Q<VisualElement>("win-screen");
            menuButton = root.Q<Button>("menu-button");
            
            menuButton.clicked += OnMenuClicked;

            _uiEventBus = ServiceLocator.ForSceneOf(this).Get<EventBusRegistry>().Get<UIEventBus>();
            _onWin = new EventBinding<OnWin>(HandleWin);
        }
    
        private void OnEnable()
        {
            _uiEventBus.Register(_onWin);
        }

        private void OnDisable()
        {
            _uiEventBus.Register(_onWin);
        }

        public void DisplayWinScreen()
        {
            screenFader.FadeOut(400, ShowWinScreen);
        }

        private void HandleWin(OnWin evt)
        {
            screenFader.FadeOut(400, ShowWinScreen);
        }

        private void ShowWinScreen()
        {
            winScreen.style.display = DisplayStyle.Flex;
            winScreen.style.opacity = 1f; // instant, screen is already black behind it
            Cursor.lockState = CursorLockMode.None;
        }

        private void HideWinScreen()
        {
            winScreen.style.opacity = 0f;
            winScreen.style.display = DisplayStyle.None;
            Time.timeScale = 1f;
        }

        private void OnMenuClicked()
        {
            HideWinScreen();
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}