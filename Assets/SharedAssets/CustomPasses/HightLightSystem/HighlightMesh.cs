using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightMesh : MonoBehaviour
{
    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private int _layerIndex = 1;

    bool _highlighted = false;

    public void ToggleHighlight()
    {
        _renderer.renderingLayerMask = (uint)(1 << (_highlighted ? 0 : _layerIndex) | 1 << 0);
        _highlighted = !_highlighted;
    }
}
