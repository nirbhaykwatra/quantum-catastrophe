using UnityEngine;

namespace SCP.Systems.SaveLoad
{
    public interface IBind<TData> where TData : ISaveable
    {
        public string Name { get; set; }
        void Bind(TData data);
    }
}
