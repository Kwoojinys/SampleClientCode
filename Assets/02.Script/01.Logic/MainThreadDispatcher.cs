using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCC
{
    //!<===================================================================================
    
    public class MainThreadDispatcher : SCC.SingletonBehaviour<MainThreadDispatcher>
    {
        //!<================================================================================

        protected readonly Queue<Action> Queued = new Queue<Action>();

        //!<================================================================================

        public bool IsRun       { get; protected set; }
        public bool IsAvailable { get; protected set; }

        //!<================================================================================
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetDomain()
        {
            
        }
        public static bool Register(Action action)
        {
            if(MainThreadDispatcher.Instance != null)
            {
                return MainThreadDispatcher.Instance
                    .InternalRegister(action);
            }

            return false;
        }

        public static void Register(IEnumerator action)
        {
            if (MainThreadDispatcher.Instance != null)
            {
                var instance = MainThreadDispatcher.Instance;
                MainThreadDispatcher
                    .Register(() => instance.StartCoroutine(action));
            }
        }
        protected bool InternalRegister(Action action)
        {
            if(this.IsRun == false)
            {
                return false;
            }

            lock (this.Queued)
            {
                this.Queued.Enqueue(action);
                return true;
            }
        }
        private void Update()
        {
            if(this.Queued.Count > 0)
            {
                lock (this.Queued)
                {
                    while (this.Queued.Count > 0)
                    {
                        this.Queued.Dequeue().Invoke();
                    }
                }
            }
        }
        public override void CheckInit()
        {
            if(this.IsAvailable == false)
            {
                this.IsAvailable = true;
                this.IsRun = true;
            }
        }
        protected override void OnBeforeDestroy()
        {
            if (this.IsAvailable == true)
            {
                this.IsAvailable = false;
                this.IsRun       = false;
            }
        }
    }
}
