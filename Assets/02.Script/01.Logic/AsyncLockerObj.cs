using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SCC;

namespace SCC.UTIL
{
    //!<===============================================================================
    //AsyncLock
    //!<===============================================================================
    public class AsyncLock : Singleton<AsyncLock>
    {
        //private ConcurrentStack<AsyncLockerObj> Pool = new();
        //private ConcurrentDictionary<string, AsyncLocker> IDLock = new();

        protected Stack<AsyncLockerObj> Pool = new();
        protected Dictionary<string, AsyncLockerObj> Locker = new();

        //!<============================================================================
        public static bool Has(string name)
            => AsyncLock.Instance?.InternalHas(name) ?? false;
        public static AsyncLockerObj Get(string name) 
            => AsyncLock.Instance?.InternalGet(name) ?? null;

        //!<============================================================================
        protected bool InternalHas(string name) => this.Locker.ContainsKey(name);
        protected AsyncLockerObj InternalGet(string name)
        {
            if(this.Locker.TryGetValue(name, out var locker) == false)
            {
                locker = this.Pool.Count > 0 ? this.Pool.Pop() :
                    new AsyncLockerObj();
                locker.Set(name, this.InternalDisposeLocker);
                this.Locker.Add(name, locker);
            }

            return locker;
        }
        protected void InternalDisposeLocker(AsyncLockerObj sender)
        {
            if(sender.IsLocked == false)
            {
                if (this.Locker.Remove(sender.KEY) == true)
                {
                    this.Pool.Push(sender);
                }
                else
                {
                    UnityEngine.Debug.LogError($"key remove from locker. key is = {sender.KEY}");
                }
            }
            else
            {
                UnityEngine.Debug.LogError("sender is Locked.");
            }
        }
    }
    //!<===============================================================================
    //AsyncLockerObj
    //!<===============================================================================
    public sealed class AsyncLockerObj
    {
        //!<===========================================================================
        
        private Action<AsyncLockerObj> __Callback = null;
        private readonly SemaphoreSlim __Semaphore = new (1, 1);
        public bool IsLocked => this.__Semaphore.CurrentCount == 0;
        public int RefCount { get; private set; } = 0;
        public string KEY   { get; private set; } = null;

        //!<===========================================================================
        public AsyncLockerObj()
        {

        }
        public AsyncLockerObj Set(string name,Action<AsyncLockerObj> dispose)
        {
            this.KEY = name;
            this.__Callback = dispose;
            return this;
        }
        public async UniTask<IDisposable> LockAsync()
        {
            this.RefCount++;
            await this.__Semaphore.WaitAsync();

            return new Handler(this.InternalRelease);
        }
        //public async UniTask WaitAsync()
        //{
        //    this.RefCount++;
        //    await this.__Semaphore.WaitAsync();

        //    this.InternalRelease();
        //}
        private void InternalRelease()
        {
            this.RefCount--;

            this.__Semaphore.Release();

            if (this.RefCount <= 0)
            {
                if (this.IsLocked == false)
                {
                    this.__Callback?.Invoke(this);
                    this.__Callback = null;
                    this.KEY = null;
                }
                else
                {
                    UnityEngine.Debug.LogError("Locker is Locked.");
                }
            }
        }
        //!<===========================================================================
        //Handler
        //!<===========================================================================
        private sealed class Handler : IDisposable
        {
            private bool __Disposed = false;
            private System.Action __Callback;

            public Handler(System.Action callback)
            {
                this.__Callback = callback;
            }
            public void Dispose()
            {
                if (this.__Disposed == false)
                {
                    this.__Callback.Invoke();
                    this.__Disposed  = true;
                }
            }
        }

        //!<===========================================================================
    }
}
