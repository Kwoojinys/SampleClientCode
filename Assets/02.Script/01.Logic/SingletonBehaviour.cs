using UnityEditor;
using UnityEngine;

namespace SCC
{
    //!<================================================================================
    public class Singleton<T> where T : Singleton<T>, new()
    {
        protected static bool isFirst = true;

        protected static T instance;
        public static T Instance
        {
            get { return instance ?? CreateInstance(); }
        }

        public static bool HasInstance { get { return instance != null; } }

        static T CreateInstance()
        {
#if UNITY_EDITOR
            if (UnityEngine.Application.isPlaying == false)
            {
                Debug.LogError(typeof(T).ToString() + ".Instance where not playing!");
                return null;
            }
#endif

            if (isFirst)
            {
                isFirst     = false;
                instance    = System.Activator.CreateInstance(typeof(T)) as T;
                instance.Init();
            }
            return instance;
        }

        protected virtual void Init()
        {

        }
        public virtual void CheckInit()
        {

        }
    }

    //!<================================================================================
    public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        //!<============================================================================

        static T instance;

        //!<============================================================================

        public static T Instance
        {
            get { return instance ?? CreateInstance(); }
        }

        public static bool HasInstance { get { return instance != null; } }
        static bool isFirst = true;

        //!<============================================================================

        static T CreateInstance()
        {
#if UNITY_EDITOR
            if (UnityEngine.Application.isPlaying == false)
            {
                Debug.LogError(typeof(T).ToString() + ".Instance where not playing!");
                return null;
            }
#endif

            if (isFirst)
            {
                isFirst = false;
                var g = new GameObject
                {
                    name = typeof(T).ToString()
                };

                GameObject.DontDestroyOnLoad(g);
                instance = g.AddComponent<T>();
                instance.Init();
            }

            return instance;
        }
        
        protected virtual void OnDestroy()
        {
            if (object.ReferenceEquals(this, instance))
            {
                this.OnBeforeDestroy();
                instance    = null;
                isFirst     = true;
            }
        }

        protected virtual void Init()
        {
        }

        protected virtual void OnBeforeDestroy()
        {
        }

        public virtual void CheckInit()
        {
        }
    }


    //!<================================================================================
    //SingletonBehaviourLimiteCycle
    //!<================================================================================
    public class SingletonBehaviourLimiteCycle<T> : MonoBehaviour where T : SingletonBehaviourLimiteCycle<T>
    {
        //!<============================================================================

        static T instance;

        //!<============================================================================

        public static T Instance
        {
            get { return instance ?? CreateInstance(); }
        }

        public static bool HasInstance { get { return instance != null; } }
        static bool isFirst = true;

        //!<============================================================================

        static T CreateInstance()
        {
#if UNITY_EDITOR
            if (UnityEngine.Application.isPlaying == false)
            {
                Debug.LogError(typeof(T).ToString() + ".Instance where not playing!");
                return null;
            }
#endif

            if (isFirst)
            {
                isFirst = false;
                var g = new GameObject
                {
                    name = typeof(T).ToString()
                };

                instance = g.AddComponent<T>();
                instance.Init();
            }

            return instance;
        }
        protected virtual void OnApplicationQuit()
        {
            
        }

        protected virtual void OnDestroy()
        {
            if (instance != null && object.ReferenceEquals(this, instance))
            {
                this.OnBeforeDestroy();

                instance = null;
                isFirst  = true;
            }
        }
        protected virtual void Init()
        {
        }

        protected virtual void OnBeforeDestroy()
        {
        }

        public virtual void CheckInit()
        {
        }
    }
}
