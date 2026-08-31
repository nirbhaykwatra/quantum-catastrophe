using System.Collections.Generic;
using UnityEngine;

namespace QC.Systems.Tutorials
{
    // Lets ScriptableObject tutorial data reference a scene object by string id,
    // since ScriptableObjects can't hold direct scene references.
    public static class TutorialTargetRegistry
    {
        private static readonly Dictionary<string, Transform> _targets = new();

        public static void Register(string id, Transform t) => _targets[id] = t;
        public static void Unregister(string id) => _targets.Remove(id);
        public static bool TryGet(string id, out Transform t) => _targets.TryGetValue(id, out t);
    }
}

