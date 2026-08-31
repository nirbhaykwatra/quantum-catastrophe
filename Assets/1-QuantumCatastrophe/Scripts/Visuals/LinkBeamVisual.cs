using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LinkBeamVisual : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    private LineRenderer _line;

    private void Awake()
    {
        _line = GetComponent<LineRenderer>();
        _line.positionCount = 2;
    }

    private void LateUpdate()
    {
        if (pointA == null || pointB == null) return;
        _line.SetPosition(0, pointA.position);
        _line.SetPosition(1, pointB.position);
    }

    public void SetEndpoints(Transform a, Transform b)
    {
        pointA = a;
        pointB = b;
    }
}