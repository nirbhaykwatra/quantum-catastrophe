using System;

namespace SCP.Systems.SaveLoad
{
    /// <summary>
    /// Serializable class which holds data for saving.
    /// To save additional data, it must be added to this class first.
    /// </summary>
    [Serializable]
    public class GameData
    {
        public string Name;
        public string CurrentLevelName;
    }
}