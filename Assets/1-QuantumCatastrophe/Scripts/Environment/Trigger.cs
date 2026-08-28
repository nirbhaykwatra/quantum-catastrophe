using UnityEngine;
using UnityEngine.Events;

public class Trigger : MonoBehaviour
{
    public UnityEvent OnTrigger;
    public bool FireOnce;

    private bool _hasBeenFired;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (FireOnce && _hasBeenFired) return;
        if (other.GetComponent<PlayerController>()) OnTrigger?.Invoke();
    }
}
