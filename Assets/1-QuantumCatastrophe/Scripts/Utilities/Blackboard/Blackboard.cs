using System;
using System.Collections.Generic;
using System.Reflection;
using QC.Utilities.Extensions;

namespace QC.Utilities.BlackboardSystem
{
    [Serializable]
    public readonly struct BlackboardKey : IEquatable<BlackboardKey>
    {
        private readonly string m_name;
        private readonly int m_hashedKey;

        public BlackboardKey(string name)
        {
            m_name = name;
            m_hashedKey = name.ComputeFNV1aHash();
        }

        public bool Equals(BlackboardKey other) => m_hashedKey == other.m_hashedKey;
        
        public override bool Equals(object obj) => obj is BlackboardKey other && Equals(other);
        public override int GetHashCode() => m_hashedKey;
        public override string ToString() => m_name;
        
        public static bool operator ==(BlackboardKey lhs, BlackboardKey rhs) => lhs.m_hashedKey == rhs.m_hashedKey;
        public static bool operator !=(BlackboardKey lhs, BlackboardKey rhs) => !(lhs == rhs);
    }

    [Serializable]
    public class BlackboardEntry<T>
    {
        public BlackboardKey Key { get; }
        public T Value { get; }
        public Type ValueType { get; }

        public BlackboardEntry(BlackboardKey key, T value)
        {
            Key = key;
            Value = value;
            ValueType = typeof(T);
        }

        public override bool Equals(object obj) => obj is BlackboardEntry<T> other && other.Key == Key;
        public override int GetHashCode() => Key.GetHashCode();
    }

    [Serializable]
    public class Blackboard
    {
        private Dictionary<string, BlackboardKey> m_keyRegistry = new();
        private Dictionary<BlackboardKey, object> m_entries = new();

        public List<Action> PassedActions { get; } = new();

        public void AddAction(Action action)
        {
            if (action == null) return;
            PassedActions.Add(action);
        }

        public void ClearActions()
        {
            PassedActions.Clear();
        }

        public void Debug()
        {
            foreach (KeyValuePair<BlackboardKey, object> entry in m_entries)
            {
                Type entryType = entry.Value.GetType();

                if (entryType.IsGenericType && entryType.GetGenericTypeDefinition() == typeof(BlackboardEntry<>))
                {
                    PropertyInfo valueProperty = entryType.GetProperty("Value");
                    if (valueProperty == null) continue;
                    object value = valueProperty.GetValue(entry.Value);
                    UnityEngine.Debug.Log($"Key: {entry.Key}, Value: {value}");
                }
            }
        }

        public bool TryGetValue<T>(BlackboardKey key, out T value)
        {
            if (m_entries.TryGetValue(key, out object entry) && entry is BlackboardEntry<T> castEntry)
            {
                value = castEntry.Value;
                return true;
            }

            value = default;
            return false;
        }

        public void SetValue<T>(BlackboardKey key, T value)
        {
            m_entries[key] = new BlackboardEntry<T>(key, value);
        }

        public BlackboardKey GetOrRegisterKey(string keyName)
        {
            if (keyName == null) return default;

            if (!m_keyRegistry.TryGetValue(keyName, out BlackboardKey key))
            {
                key = new BlackboardKey(keyName);
                m_keyRegistry[keyName] = key;
            }

            return key;
        }

        public bool ContainsKey(BlackboardKey key) => m_entries.ContainsKey(key);

        public void RemoveKey(BlackboardKey key) => m_entries.Remove(key);
    }
}