using UnityEngine;

namespace QC.Systems.Tutorials
{
    // Drop this on any scene object a tutorial step should be able to highlight.
    public class TutorialTargetMarker : MonoBehaviour
    {
        [SerializeField] private string targetId;

        private void OnEnable() => TutorialTargetRegistry.Register(targetId, transform);
        private void OnDisable() => TutorialTargetRegistry.Unregister(targetId);
    }
}