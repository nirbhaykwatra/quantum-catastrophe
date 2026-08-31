using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class Trigger : MonoBehaviour
{
    [ReadOnly] [ShowInInspector] private List<GameObject> _overlappingObjects = new();
    
    [SerializeField] private UnityEvent OnTrigger;
    [SerializeField] private bool FireOnce;
    
    private bool _hasBeenFired;

    public IReadOnlyList<GameObject> OverlappingObjects => _overlappingObjects;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (FireOnce && _hasBeenFired) return;
        if (!_overlappingObjects.Contains(other.gameObject)) _overlappingObjects.Add(other.gameObject);
        if (other.GetComponent<PlayerController>()) OnTrigger?.Invoke();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_overlappingObjects.Contains(other.gameObject)) _overlappingObjects.Remove(other.gameObject);
    }
}
