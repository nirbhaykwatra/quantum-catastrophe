using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LevelSelectScreen : MenuScreen
{
    [SerializeField] private LevelRegistry _levelRegistry;
    [SerializeField] private VisualTreeAsset _levelSelectItemTemplate;

    private Button _backButton;

    protected override void RegisterCallbacks(VisualElement tree)
    {
        _backButton = tree.Q<Button>("btn-back");
        _backButton.RegisterCallback<ClickEvent>(OnBackClicked);
    }

    protected override void OnShow(VisualElement tree)
    {
        VisualElement grid = tree.Q<VisualElement>("level-grid");

        foreach (LevelData level in _levelRegistry.Levels)
        {
            TemplateContainer item = _levelSelectItemTemplate.Instantiate();
            Debug.Log($"item: {item.Q<Button>("level-select-item-button")}");
            Button btn = item.Q<Button>("level-select-item-button");
            btn.text = level.IsLocked ? "Locked" : level.LevelName.SplitPascalCase();
            btn.style.backgroundImage = new StyleBackground(level.Thumbnail);
            btn.style.unityBackgroundImageTintColor = level.IsLocked ? Color.grey : level.ThumbnailTint;
            btn.RegisterCallback<ClickEvent>(_ => OnLevelClicked(level));

            grid.Add(btn);
        }
    }

    protected override void UnregisterCallbacks(VisualElement tree)
    {
        _backButton.UnregisterCallback<ClickEvent>(OnBackClicked);
    }

    private void OnBackClicked(ClickEvent evt)
    {
        MenuManager.Pop();
    }

    private void OnLevelClicked(LevelData level)
    {
        Debug.Log($"Loading {level.LevelName}");
        SceneManager.LoadScene(level.LevelName);
    }
}