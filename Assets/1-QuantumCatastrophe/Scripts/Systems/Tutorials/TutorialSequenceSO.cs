// TutorialSequenceSO.cs

using Sirenix.OdinInspector;
using UnityEngine;

namespace QC.Systems.Tutorials
{
    [CreateAssetMenu(menuName = "Tutorials/Tutorial Sequence")]
    public class TutorialSequenceSO : ScriptableObject
    {
        [Tooltip("Unique id used to track whether this tutorial has already been completed.")]
        public string tutorialId;

        public TutorialStep[] steps;
    }

    public enum HighlightMode { None, UIElement, WorldObject }

    public enum TutorialAdvanceMode { ButtonClick, WaitForEvent, Timer }

    // Flipbook plays a pre-sliced Sprite[] (frames exported from a source GIF via
    // ffmpeg/ImageMagick/etc.) on a timer — no VideoPlayer/VideoClip involved, so
    // it behaves identically on every platform, WebGL included.
    public enum IllustrationMode { None, Sprite, Flipbook }

    [System.Serializable]
    public class TutorialStep
    {
        [Header("Content")]
        public string headerText;
        [TextArea] public string bodyText;

        [Header("Illustration")]
        public IllustrationMode illustrationMode;
        
        [Tooltip("Used when illustrationMode == Sprite.")]
        public Sprite illustration;

        [Tooltip("Used when illustrationMode == Flipbook. Frames in playback order, " +
                 "sliced from the source GIF ahead of time.")]
        public Sprite[] flipbookFrames;

        [Tooltip("Playback speed for flipbookFrames, in frames per second.")]
        public float flipbookFrameRate = 12f;

        [Header("Highlight")]
        public HighlightMode highlightMode;

        [Tooltip("If highlightMode == UIElement, this is the target VisualElement's name.\n" +
                 "If highlightMode == WorldObject, this is the id registered via TutorialTargetMarker.")]
        public string highlightTargetId;

        [Tooltip("Only used when highlightMode == WorldObject.")]
        public float worldHighlightRadius = 60f;

        [Header("Advancement")]
        public TutorialAdvanceMode advanceMode;

        [Tooltip("Only used when advanceMode == WaitForEvent. Must match the signalName\n" +
                 "published via GameplaySignalEvent (e.g. from TutorialSignalInteractBehavior).")]
        public string requiredEventName;

        [Tooltip("Only used when advanceMode == Timer.")]
        public float timerSeconds;
    }
}