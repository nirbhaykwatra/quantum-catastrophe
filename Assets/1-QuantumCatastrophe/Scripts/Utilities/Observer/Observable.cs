using System;
using System.Collections.Generic;

namespace QC.Utilities.ObserverSystem
{
    [Serializable]
    public class Observable<T>
    {
        private T m_value;
        public event Action<T> ValueChanged;

        public T Value
        {
            get => m_value;
            set => Set(value);
        }

        public static implicit operator T(Observable<T> observable)
        {
            return observable.m_value;
        }

        public Observable(T value, Action<T> onValueChanged = null)
        {
            m_value = value;

            if (onValueChanged != null)
            {
                ValueChanged += onValueChanged;
            }
        }

        public void Set(T value)
        {
            if (EqualityComparer<T>.Default.Equals(m_value, value))
            {
                return;
            }

            m_value = value;
            Invoke();
        }

        public void Invoke()
        {
            ValueChanged?.Invoke(m_value);
        }

        public void AddListener(Action<T> handler)
        {
            ValueChanged += handler;
        }

        public void RemoveListener(Action<T> handler)
        {
            ValueChanged -= handler;
        }

        public void Dispose()
        {
            ValueChanged = null;
            m_value = default;
        }
    }
}