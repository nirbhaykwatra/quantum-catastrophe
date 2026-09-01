using System;
using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;
using UnityEngine;

public class PowerUnit : MonoBehaviour
{
    private Vector2 _startPosition;
    private GlobalEventBus _globalEventBus;
    private EventBinding<OnResetLevel> _resetBinding;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _globalEventBus = ServiceLocator.ForSceneOf(this).Get<EventBusRegistry>().Get<GlobalEventBus>();
        _rb = GetComponent<Rigidbody2D>();
        _startPosition = transform.position;
    }

    private void OnEnable()
    {
        _resetBinding = new EventBinding<OnResetLevel>(ResetPosition);
        _globalEventBus.Register(_resetBinding);
    }

    private void OnDisable()
    {
        _globalEventBus.Deregister(_resetBinding);
    }

    private void ResetPosition()
    {
        _rb.MovePosition(_startPosition);
        _rb.MoveRotation(0);
    }
}