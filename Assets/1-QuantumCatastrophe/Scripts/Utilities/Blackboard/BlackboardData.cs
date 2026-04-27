using System;
using System.Collections.Generic;
using UnityEngine;

namespace QC.Utilities.BlackboardSystem
{
    [CreateAssetMenu(fileName = "New Blackboard Data", menuName = "QC/Blackboard/Blackboard Data", order = 0)]
    public class BlackboardData : ScriptableObject 
    {
        public List<BlackboardEntryData> Entries = new();

        public void SetValuesOnBlackboard(Blackboard blackboard)
        {
            foreach (BlackboardEntryData entry in Entries)
            {
                entry.SetValueOnBlackboard(blackboard);
            }
        }

        public void SetValuesInDataObject(Blackboard fromBlackboard)
        {
            foreach (BlackboardEntryData entry in Entries)
            {
                BlackboardKey searchKey = new(entry.keyName);
                if (fromBlackboard.TryGetValue(searchKey, out AnyValue value))
                {
                    entry.value = value;
                }
            }
        }
    }

    [Serializable]
    public class BlackboardEntryData : ISerializationCallbackReceiver
    {
        public string keyName;
        public AnyValue.ValueType valueType;
        public AnyValue value;

        public void SetValueOnBlackboard(Blackboard blackboard)
        {
            BlackboardKey key = blackboard.GetOrRegisterKey(keyName);
            s_setValueDispatchTable[value.type](blackboard, key, value);
        }

        // Dispatch table to set different types of value on the blackboard
        private static Dictionary<AnyValue.ValueType, Action<Blackboard, BlackboardKey, AnyValue>>
            s_setValueDispatchTable = new()
            {
                { AnyValue.ValueType.Int, (blackboard, key, anyValue) => blackboard.SetValue<int>(key, anyValue) },
                { AnyValue.ValueType.Float, (blackboard, key, anyValue) => blackboard.SetValue<float>(key, anyValue) },
                { AnyValue.ValueType.Bool, (blackboard, key, anyValue) => blackboard.SetValue<bool>(key, anyValue) },
                {
                    AnyValue.ValueType.String, (blackboard, key, anyValue) => blackboard.SetValue<string>(key, anyValue)
                },
                {
                    AnyValue.ValueType.Vector3,
                    (blackboard, key, anyValue) => blackboard.SetValue<Vector3>(key, anyValue)
                },
            };

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize() => value.type = valueType;
    }

    [Serializable]
    public struct AnyValue
    {
        public enum ValueType
        {
            Int,
            Float,
            Bool,
            String,
            Vector3
        }

        public ValueType type;

        // Storage for different types of values
        public int intValue;
        public float floatValue;
        public bool boolValue;
        public string stringValue;
        public Vector3 vector3Value;
        // Add more types as needed, but remember to add them to the dispatch table above and the custom Editor

        // Implicit conversion operators to convert AnyValue to different types
        public static implicit operator int(AnyValue value) => value.ConvertValue<int>();
        public static implicit operator float(AnyValue value) => value.ConvertValue<float>();
        public static implicit operator bool(AnyValue value) => value.ConvertValue<bool>();
        public static implicit operator string(AnyValue value) => value.ConvertValue<string>();
        public static implicit operator Vector3(AnyValue value) => value.ConvertValue<Vector3>();

        private T ConvertValue<T>()
        {
            return type switch
            {
                ValueType.Int => AsInt<T>(intValue),
                ValueType.Float => AsFloat<T>(floatValue),
                ValueType.Bool => AsBool<T>(boolValue),
                ValueType.String => (T)(object)stringValue,
                ValueType.Vector3 => AsVector3<T>(vector3Value),
                _ => throw new NotSupportedException($"Not supported value type: {typeof(T)}")
            };
        }

        // Helper methods for safe type conversions of the value types without the cost of boxing
        T AsBool<T>(bool value) => typeof(T) == typeof(bool) && value is T correctType ? correctType : default;
        T AsInt<T>(int value) => typeof(T) == typeof(int) && value is T correctType ? correctType : default;
        T AsFloat<T>(float value) => typeof(T) == typeof(float) && value is T correctType ? correctType : default;
        T AsVector3<T>(Vector3 value) => typeof(T) == typeof(Vector3) && value is T correctType ? correctType : default;
    }
}