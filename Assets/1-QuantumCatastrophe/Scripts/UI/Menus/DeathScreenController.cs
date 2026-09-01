using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class DeathScreenController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private ScreenFader screenFader;
    
    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string currentSceneName;

    private VisualElement deathScreen;
    private Button retryButton;
    private Button menuButton;

    private UIEventBus _uiEventBus;
    private EventBinding<OnDeath> _onDeath;

    private void Awake()
    {
        VisualElement root = uiDocument.rootVisualElement;
        deathScreen = root.Q<VisualElement>("death-screen");
        retryButton = root.Q<Button>("retry-button");
        menuButton = root.Q<Button>("menu-button");

        retryButton.clicked += OnRetryClicked;
        menuButton.clicked += OnMenuClicked;

        _uiEventBus = ServiceLocator.ForSceneOf(this).Get<EventBusRegistry>().Get<UIEventBus>();
        _onDeath = new EventBinding<OnDeath>(HandleDeath);
    }
    
    private void OnEnable()
    {
        _uiEventBus.Register(_onDeath);
    }

    private void OnDisable()
    {
        _uiEventBus.Register(_onDeath);
    }

    private void HandleDeath(OnDeath evt)
    {
        screenFader.FadeOut(400, ShowDeathScreen);
    }

    private void ShowDeathScreen()
    {
        deathScreen.style.display = DisplayStyle.Flex;
        deathScreen.style.opacity = 1f; // instant, screen is already black behind it
        Cursor.lockState = CursorLockMode.None;
    }

    private void HideDeathScreen()
    {
        deathScreen.style.opacity = 0f;
        deathScreen.style.display = DisplayStyle.None;
        Time.timeScale = 1f;
    }

    private void OnRetryClicked()
    {
        HideDeathScreen();
        SceneManager.LoadScene(currentSceneName);
    }

    private void OnMenuClicked()
    {
        HideDeathScreen();
        SceneManager.LoadScene(mainMenuSceneName);
    }
}