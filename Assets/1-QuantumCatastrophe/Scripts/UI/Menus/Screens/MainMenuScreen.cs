using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuScreen : MenuScreen
{
    [SerializeField] private LevelSelectScreen _levelSelectScreen;
    [SerializeField] private SettingsScreen _settingsScreen;

    private Button _playButton;
    private Button _settingsButton;
    private Button _quitButton;

    protected override void RegisterCallbacks(VisualElement tree)
    {
        _playButton = tree.Q<Button>("btn-select-level");
        _settingsButton = tree.Q<Button>("btn-options");
        _quitButton = tree.Q<Button>("btn-credits");

        _playButton.RegisterCallback<ClickEvent>(OnPlayClicked);
        _settingsButton.RegisterCallback<ClickEvent>(OnSettingsClicked);
        _quitButton.RegisterCallback<ClickEvent>(OnQuitClicked);
    }

    protected override void UnregisterCallbacks(VisualElement tree)
    {
        _playButton.UnregisterCallback<ClickEvent>(OnPlayClicked);
        _settingsButton.UnregisterCallback<ClickEvent>(OnSettingsClicked);
        _quitButton.UnregisterCallback<ClickEvent>(OnQuitClicked);
    }

    private void OnPlayClicked(ClickEvent evt)
    {
        MenuManager.Push(_levelSelectScreen);
    }

    private void OnSettingsClicked(ClickEvent evt)
    {
        MenuManager.Push(_settingsScreen);
    }

    private void OnQuitClicked(ClickEvent evt)
    {
        Application.Quit();
    }
}