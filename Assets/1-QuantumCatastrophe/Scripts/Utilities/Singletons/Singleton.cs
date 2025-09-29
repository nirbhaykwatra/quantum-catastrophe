using UnityEngine;

namespace SCP.Utilities
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        protected static T instance;
        
        public static bool HasInstance => instance != null;
        public static T TryGetInstance() => HasInstance ? instance : null;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<T>();
                    if (instance == null)
                    {
                        GameObject gameObject = new GameObject(typeof(T).Name + " Default");
                        instance = gameObject.AddComponent<T>();
                    }
                }
                return instance;
            }
        }
        
        protected virtual void Awake()
        {
            InitializeSingleton();
        }

        protected virtual void InitializeSingleton()
        {
            if (!Application.isPlaying) return;

            if (instance == null)
            {
                instance = this as T;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
    
}
