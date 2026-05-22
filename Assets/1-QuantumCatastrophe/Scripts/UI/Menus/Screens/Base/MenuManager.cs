using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private UIDocument _UIDocument;

    // Assign all screens in the Inspector
    [SerializeField] private MainMenuScreen _mainMenuScreen;
    [SerializeField] private SettingsScreen _settingsScreen;
    [SerializeField] private LevelSelectScreen _levelSelectScreen;

    private Stack<MenuScreen> _screenStack = new();
    private VisualElement _root;

    private void Awake()
    {
        _root = _UIDocument.rootVisualElement.Q("root-container");

        // Initialize all screens with a reference to this manager and the root
        _mainMenuScreen.Initialize(this, _root);
        _settingsScreen.Initialize(this, _root);
        _levelSelectScreen.Initialize(this, _root);
    }

    private void Start()
    {
        Push(_mainMenuScreen);
    }

    public void Push(MenuScreen screen)
    {
        if (_screenStack.TryPeek(out MenuScreen current))
        {
            current.Hide();
        }

        _screenStack.Push(screen);
        screen.Show();
    }

    public void Pop()
    {
        if (_screenStack.Count <= 1) return; // Don't pop the last screen

        _screenStack.Pop().Hide();

        if (_screenStack.TryPeek(out MenuScreen previous))
        {
            previous.Show();
        }
    }
}