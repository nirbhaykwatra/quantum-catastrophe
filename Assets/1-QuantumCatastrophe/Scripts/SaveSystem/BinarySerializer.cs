using System;
using System.Text;
using Sirenix.Serialization;

namespace SCP.Systems.SaveLoad
{
    public class BinarySerializer
    {
        public byte[] Serialize<T>(T obj)
        {
            return SerializationUtility.SerializeValue(obj, DataFormat.Binary);
        }
        
        public T Deserialize<T>(byte[] bytes)
        {
            return SerializationUtility.DeserializeValue<T>(bytes, DataFormat.Binary);
        }
    }
}