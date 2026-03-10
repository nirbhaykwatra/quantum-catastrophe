using UnityEngine;
using UnityEngine.Events;

namespace QC.Utilities.ObserverSystem
{
    public class Observer<T>
    {
        [SerializeField] T m_value;
        [SerializeField] UnityEvent<T> m_onValueChanged;

        public T Value
        {
            get => m_value;
            set => Set(value);
        }

        public Observer(T value, UnityAction<T> callback = null)
        {
            m_value = value;
            m_onValueChanged = new UnityEvent<T>();
            if (callback != null) m_onValueChanged.AddListener(callback);
        }

        public void Set(T value)
        {
            if (Equals(m_value, value)) return;
            m_value = value;
            Invoke();
        }

        public void Invoke()
        {
            Debug.Log($"Invoking {m_onValueChanged.GetPersistentEventCount()} listeners");
            m_onValueChanged.Invoke(m_value);
        }
        
        public void AddListener(UnityAction<T> callback)
        {
            if (callback == null) return;
            if (m_onValueChanged == null) m_onValueChanged = new UnityEvent<T>();
            m_onValueChanged.AddListener(callback);
        }
        
        public void RemoveListener(UnityAction<T> callback)
        {
            if (callback == null) return;
            if (m_onValueChanged == null) return;
            m_onValueChanged.RemoveListener(callback);
        }
        
        public void RemoveAllListeners()
        {
            if (m_onValueChanged == null) return;
            m_onValueChanged.RemoveAllListeners();
        }

        public void Dispose()
        {
            RemoveAllListeners();
            m_onValueChanged = null;
            m_value = default;
        }
        
    }
}