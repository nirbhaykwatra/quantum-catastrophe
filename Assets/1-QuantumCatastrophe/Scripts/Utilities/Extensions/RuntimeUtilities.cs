// RuntimeUtilities.cs
using UnityEngine;
using UnityEngine.UIElements;

public static class RuntimeUtilities
{
    // UI Toolkit panel space has Y flipped relative to screen space (origin top-left,
    // Y increases downward), so screen-space Y needs flipping before the panel conversion.
    public static Vector2 CameraToPanelPoint(Camera cam, Vector3 worldPos, IPanel panel)
    {
        Vector3 screenPoint = cam.WorldToScreenPoint(worldPos);
        Vector2 flipped = new Vector2(screenPoint.x, Screen.height - screenPoint.y);
        return RuntimePanelUtils.ScreenToPanel(panel, flipped);
    }
}
