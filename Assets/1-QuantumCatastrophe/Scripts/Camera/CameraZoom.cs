using System;
using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;
using UnityEngine;
using UnityEngine.InputSystem; // Required for InputValue
using Unity.Cinemachine;

public class CameraZoom : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private CinemachineCamera gameplayCamera;
    [SerializeField] private CinemachineCamera zoomOutCamera;

    [Header("Priority Settings")]
    [SerializeField] private int activePriority = 15;
    [SerializeField] private int inactivePriority = 10;

    private GlobalEventBus _globalEventBus;
    private EventBinding<OnZoomOut> _zoomOutBinding;
    private bool _zoomed;

    private void Awake()
    {
        _globalEventBus = ServiceLocator.ForSceneOf(this).Get<EventBusRegistry>().Get<GlobalEventBus>();
    }

    private void OnEnable()
    {
        _zoomOutBinding = new EventBinding<OnZoomOut>(HandleZoomOut);
        _globalEventBus.Register(_zoomOutBinding);
    }

    private void OnDisable()
    {
        _globalEventBus.Deregister(_zoomOutBinding);
    }
    
    private void HandleZoomOut(OnZoomOut value)
    {
        _zoomed = !_zoomed;
        
        if (_zoomed)
        {
            HoldZoomOut();
        }
        else
        {
            ReleaseZoomIn();
        }
    }

    private void HoldZoomOut()
    {
        zoomOutCamera.Priority = activePriority;
        gameplayCamera.Priority = inactivePriority;
    }

    private void ReleaseZoomIn()
    {
        gameplayCamera.Priority = activePriority;
        zoomOutCamera.Priority = inactivePriority;
    }
}