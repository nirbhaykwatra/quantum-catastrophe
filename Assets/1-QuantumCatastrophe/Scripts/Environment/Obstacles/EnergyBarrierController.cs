using System.Collections;
using UnityEngine;

/// <summary>
/// Drives a MeshRenderer/SpriteRenderer using the Custom/EnergyBarrier shader.
/// Handles color swapping, fade in/out, and a BoxCollider2D trigger toggle
/// so the barrier can block or allow the player through.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class EnergyBarrierController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Renderer barrierRenderer;
    [SerializeField] private BoxCollider2D barrierCollider; // optional, only needed if barrier blocks movement

    [Header("Color Presets")]
    [SerializeField] private Color activeMainColor = new Color(0.2f, 0.6f, 1f, 0.5f);
    [SerializeField] private Color activeEdgeColor = new Color(0.6f, 0.9f, 1f, 1f);
    [SerializeField] private Color disabledMainColor = new Color(1f, 0.2f, 0.2f, 0.35f);
    [SerializeField] private Color disabledEdgeColor = new Color(1f, 0.5f, 0.4f, 1f);

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.4f;

    private static readonly int MainColorID = Shader.PropertyToID("_MainColor");
    private static readonly int EdgeColorID = Shader.PropertyToID("_EdgeColor");
    private static readonly int BaseAlphaID = Shader.PropertyToID("_BaseAlpha");

    private MaterialPropertyBlock _propBlock;
    private Coroutine _fadeRoutine;
    private float _targetBaseAlpha = 0.45f;

    public bool IsActive { get; private set; } = true;

    private void Awake()
    {
        if (barrierRenderer == null)
            barrierRenderer = GetComponent<Renderer>();

        _propBlock = new MaterialPropertyBlock();
        ApplyColors(activeMainColor, activeEdgeColor);
    }

    // Lets you tweak the color fields in the Inspector (in Edit Mode or Play Mode) and see
    // the barrier update immediately, instead of only applying once in Awake().
    private void OnValidate()
    {
        if (barrierRenderer == null)
            barrierRenderer = GetComponent<Renderer>();
        if (barrierRenderer == null || _propBlock == null)
            _propBlock ??= new MaterialPropertyBlock();

        ApplyColors(IsActive ? activeMainColor : disabledMainColor,
                    IsActive ? activeEdgeColor : disabledEdgeColor);
    }

    /// <summary>Swap to the "safe/off" color scheme without disabling collision.</summary>
    public void SetDisabledLook()
    {
        ApplyColors(disabledMainColor, disabledEdgeColor);
    }

    /// <summary>Swap back to the normal "blocking" color scheme.</summary>
    public void SetActiveLook()
    {
        ApplyColors(activeMainColor, activeEdgeColor);
    }

    /// <summary>Fades the barrier out and disables its collider (lets the player pass).</summary>
    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        if (barrierCollider != null) barrierCollider.enabled = false;
        StartFade(0f);
    }

    /// <summary>Fades the barrier back in and re-enables its collider.</summary>
    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        StartFade(_targetBaseAlpha);
        if (barrierCollider != null) barrierCollider.enabled = true;
    }

    private void ApplyColors(Color mainColor, Color edgeColor)
    {
        barrierRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(MainColorID, mainColor);
        _propBlock.SetColor(EdgeColorID, edgeColor);
        barrierRenderer.SetPropertyBlock(_propBlock);
    }

    private void StartFade(float targetAlpha)
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        barrierRenderer.GetPropertyBlock(_propBlock);
        float startAlpha = _propBlock.GetFloat(BaseAlphaID);
        if (startAlpha <= 0f) startAlpha = _targetBaseAlpha; // first run, prop block has no override yet

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float lerped = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            barrierRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(BaseAlphaID, lerped);
            barrierRenderer.SetPropertyBlock(_propBlock);
            yield return null;
        }

        barrierRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetFloat(BaseAlphaID, targetAlpha);
        barrierRenderer.SetPropertyBlock(_propBlock);
    }
}
