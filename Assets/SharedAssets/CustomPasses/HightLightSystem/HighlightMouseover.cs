using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightMouseover : MonoBehaviour
{
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(mouseRay, out RaycastHit hit, Mathf.Infinity))
            {
                if(hit.collider.TryGetComponent(out HighlightMesh highlightMesh)) highlightMesh.ToggleHighlight();
            }
        }
    }
}