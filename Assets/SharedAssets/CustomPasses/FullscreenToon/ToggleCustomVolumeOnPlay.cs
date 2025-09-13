#if HDRP
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(CustomPassVolume))]

public class ToggleCustomVolumeOnPlay : MonoBehaviour
{
    [SerializeField] private CustomPassVolume _volume;

    private void OnValidate()
    {
        _volume = GetComponent<CustomPassVolume>();
    }

    private void OnEnable()
    {
        _volume.enabled = true;
    }

    private void OnDisable()
    {
        _volume.enabled = false;
    }
}
#endif