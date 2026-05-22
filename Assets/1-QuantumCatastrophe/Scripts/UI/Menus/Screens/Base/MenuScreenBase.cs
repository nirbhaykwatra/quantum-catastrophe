using UnityEngine;
using UnityEngine.UIElements;

public abstract class MenuScreen : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset _uxml;

    protected MenuManager MenuManager { get; private set; }
    protected VisualElement Root { get; private set; }
    private VisualElement _tree;

    public void Initialize(MenuManager manager, VisualElement root)
    {
        MenuManager = manager;
        Root = root;
    }

    public void Show()
    {
        _tree = _uxml.Instantiate();
        _tree.style.flexGrow = 1;
        _tree.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
        _tree.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
        Root.Add(_tree);
        RegisterCallbacks(_tree);
        OnShow(_tree);
    }

    public void Hide()
    {
        OnHide(_tree);
        UnregisterCallbacks(_tree);
        Root.Remove(_tree);
        _tree = null;
    }

    // Override these in each screen
    protected virtual void RegisterCallbacks(VisualElement tree)
    {
    }

    protected virtual void UnregisterCallbacks(VisualElement tree)
    {
    }

    protected virtual void OnShow(VisualElement tree)
    {
    }

    protected virtual void OnHide(VisualElement tree)
    {
    }
}