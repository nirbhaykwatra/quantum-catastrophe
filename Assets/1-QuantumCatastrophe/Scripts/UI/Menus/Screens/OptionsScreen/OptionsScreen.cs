using UnityEngine;
using UnityEngine.UIElements;

public class SettingsScreen : MenuScreen
{
    private Button _backButton;

    protected override void RegisterCallbacks(VisualElement tree)
    {
        _backButton = tree.Q<Button>("btn-back");
        _backButton.RegisterCallback<ClickEvent>(OnBackClicked);
    }

    protected override void UnregisterCallbacks(VisualElement tree)
    {
        _backButton.UnregisterCallback<ClickEvent>(OnBackClicked);
    }

    private void OnBackClicked(ClickEvent evt)
    {
        MenuManager.Pop();
    }
}