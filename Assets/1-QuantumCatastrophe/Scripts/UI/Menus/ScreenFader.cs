using System;
using System.Collections.Generic;
using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;
using UnityEngine;
using UnityEngine.UIElements;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Color fadeColor = Color.black;
    private VisualElement fadeOverlay;
    private UIEventBus _uiEventBus;
    private EventBinding<OnScreenFade> _onScreenFade;

    private void Awake()
    {
        VisualElement root = uiDocument.rootVisualElement;

        fadeOverlay = new VisualElement();
        fadeOverlay.AddToClassList("fade-overlay");
        fadeOverlay.style.position = Position.Absolute;
        fadeOverlay.style.left = 0;
        fadeOverlay.style.right = 0;
        fadeOverlay.style.top = 0;
        fadeOverlay.style.bottom = 0;
        fadeOverlay.style.backgroundColor = fadeColor;
        fadeOverlay.style.opacity = 0f;
        fadeOverlay.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("opacity") };
        fadeOverlay.style.transitionTimingFunction = new List<EasingFunction> { new EasingFunction(EasingMode.EaseInOut) };
        fadeOverlay.pickingMode = PickingMode.Ignore;
        root.Add(fadeOverlay); // add last so it's on top

        _uiEventBus = ServiceLocator.ForSceneOf(this).Get<EventBusRegistry>().Get<UIEventBus>();
    }

    private void OnEnable()
    {
        _onScreenFade = new EventBinding<OnScreenFade>(HandleFade);
        _uiEventBus.Register(_onScreenFade);
    }

    private void OnDisable()
    {
        _uiEventBus.Register(_onScreenFade);
    }

    private void HandleFade(OnScreenFade @event)
    {
        FadeOutThenIn(@event.FadeOutMs, @event.HoldMs, @event.FadeInMs,  @event.OnBlack, @event.OnComplete);
    }
    
    /// <summary>Fades the overlay to fully opaque (screen goes black). Blocks input while faded.</summary>
    public void FadeOut(float durationMs, Action onComplete = null)
    {
        fadeOverlay.pickingMode = PickingMode.Position;
        SetTransitionDuration(durationMs);
        fadeOverlay.style.opacity = 1f;

        fadeOverlay.schedule.Execute(() => onComplete?.Invoke())
            .StartingIn((long)durationMs);
    }

    /// <summary>Fades the overlay back to fully transparent. Restores input on completion.</summary>
    public void FadeIn(float durationMs, Action onComplete = null)
    {
        SetTransitionDuration(durationMs);
        fadeOverlay.style.opacity = 0f;

        fadeOverlay.schedule.Execute(() =>
        {
            fadeOverlay.pickingMode = PickingMode.Ignore;
            onComplete?.Invoke();
        }).StartingIn((long)durationMs);
    }

    /// <summary>
    /// Convenience wrapper for quick flash-style transitions: fade out, hold, fade back in.
    /// For screens that should STAY black until something else triggers FadeIn
    /// (e.g. a death screen), use FadeOut(...) alone instead.
    /// </summary>
    public void FadeOutThenIn(float fadeOutMs, float holdMs, float fadeInMs, Action onBlack = null, Action onComplete = null)
    {
        FadeOut(fadeOutMs, () =>
        {
            onBlack?.Invoke();

            fadeOverlay.schedule.Execute(() => FadeIn(fadeInMs, onComplete))
                .StartingIn((long)holdMs);
        });
    }

    /// <summary>True while the overlay is at or animating toward fully opaque.</summary>
    public bool IsBlack => Mathf.Approximately(fadeOverlay.resolvedStyle.opacity, 1f);

    private void SetTransitionDuration(float durationMs)
    {
        fadeOverlay.style.transitionDuration = new List<TimeValue> { new TimeValue(durationMs, TimeUnit.Millisecond) };
    }
}