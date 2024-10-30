using System;
using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace SCC
{
    public interface IAddressableObjectPoolRelease<T>
   where T : UnityEngine.Component
    {
        bool Release(T @object);
    }
    public interface IAddressableObjectPoolCreate<T>
       where T : UnityEngine.Component
    {
        T CreateInstance(Transform parent = null);
    }

    //!<=====================================================================================
    public interface IAddressableObjectPool<T>: IAddressableObjectPoolRelease<T>, IAddressableObjectPoolCreate<T>
        where T : UnityEngine.Component
    {
        bool IsValidRef { get; }
    }

    //!<=====================================================================================

    //public class PooledObject<T> : IDisposable where T :  UnityEngine.Component
    //{
    //    //!<==================================================================================

    //    public  readonly T Instance;
    //    private readonly IAddressableObjectPool<T> Pool;
    //    private bool IsDispose;

    //    //!<==================================================================================

    //    internal PooledObject(T value, IAddressableObjectPool<T> pool)
    //    {
    //        this.Instance   = value;
    //        this.Pool       = pool;
    //        this.IsDispose  = false;
    //    }
    //    public void Dispose()
    //    {
    //        if(this.Instance != null && this.IsDispose == false)
    //        {
    //            this.Pool.Release(this.Instance);
    //        }

    //        this.IsDispose = true;
    //    }
    //}
    //!<=====================================================================================
    //AddressableAssetReferenceT
    //!<=====================================================================================
    [Serializable]
    public class AddressableAssetReferenceObject<T> :
        IDisposable, IAddressableObjectPool<T>
        where T : UnityEngine.Component
    {
        //!<==================================================================================

        [SerializeField]public string AssetGUID                    { get; protected set; }

        //!<==================================================================================

        public IAddressableLoadedAsset LoadedAsset { get; protected set; }
        public bool IsLoaded => this.LoadedAsset?.IsValid ?? false;
        public bool IsUsingObjectPool               { get; protected set; } = false;
        public bool IsValidRef => string.IsNullOrEmpty(this.AssetGUID) == false;

        //!<==================================================================================

        protected HashSet<T>  ObjectPoolActive;
        protected List<T>     ObjectPoolInActive;
        private bool DisposedValue = false;

        //!<==================================================================================
        public AddressableAssetReferenceObject<T> InitAssetGUID(string guid)
        {
            if(this.AssetGUID != guid)
            {
                this.DoCleanup();
            }

            this.AssetGUID = guid;
            return this;
        }
        protected virtual void Dispose(bool disposing)
        {
            if (this.DisposedValue == false)
            {
                if (disposing)
                {
                    this.DoCleanup();
                }
                this.DisposedValue = true;
            }
        }
        protected T InternalGetPooledObject()
        {
            if(this.IsUsingObjectPool == true && 
                this.ObjectPoolInActive?.Count > 0)
            {
                var index   = this.ObjectPoolInActive.Count - 1;
                var val     = this.ObjectPoolInActive[index];
                this.ObjectPoolInActive.RemoveAt(index);

                return val;
            }

            return null;
        }
        public IAddressableObjectPool<T> EnablePooling(int capacity = 20)
        {
            if(this.IsUsingObjectPool == false)
            {
                this.ObjectPoolActive   ??= new HashSet<T>();
                this.ObjectPoolInActive ??= new List<T>() { Capacity = capacity, };

                this.IsUsingObjectPool = true;
            }
            return this;
        }
        public void DisablePooling()
        {
            this.IsUsingObjectPool = false;
            this.DoCleanup();
        }
        public void ReleasePooledObject(T element)
        {
            if(this.IsUsingObjectPool == true)
            {
                if(element.gameObject != null)
                {
                    element.gameObject.SetActive(false);

                    if(this.ObjectPoolActive.Remove(element) == true)
                    {
                        this.ObjectPoolInActive.Add(element);
                    }
                    else
                    {
                        Debug.LogError($"error element contain[{element}]");
                    }
                }
            }
        }
        public bool ValidateAsset(UnityEngine.Object obj)
        {
            var type = obj.GetType();
            return typeof(T).IsAssignableFrom(type);
        }
        public void DoCleanup()
        {
            if(this.IsUsingObjectPool == true)
            {
                if (SCC.Application.IsAvailable == true)
                {
                    if(this.ObjectPoolActive?.Count > 0)
                    {
                        foreach (var i in this.ObjectPoolActive)
                        {
                            UnityEngine.GameObject.DestroyImmediate(i.gameObject);
                        }
                    }
                    if(this.ObjectPoolInActive?.Count > 0)
                    {
                        foreach (var i in this.ObjectPoolInActive)
                        {
                            UnityEngine.GameObject.DestroyImmediate(i.gameObject);
                        }
                    }
                    this.ObjectPoolInActive.Clear();
                    this.ObjectPoolActive.Clear();
                }
            }
            if(this.LoadedAsset != null)
            {
                if (AddressableAssetsService.IsAvailable == true)
                {
                    AddressableAssetsService.Instance
                        .DecrementReferenceCount(this.LoadedAsset);
                }
            }
            
            this.IsUsingObjectPool  = false;
            this.LoadedAsset        = null;
        }
        public async UniTask<bool> PreLoadAsync()
        {
            if (AddressableAssetsService.IsAvailable == false)
            {
                return false;
            }
            try
            {
                if (this.LoadedAsset == null || this.LoadedAsset.IsValid == false)
                {
                    var @ref = await AddressableAssetsService.Instance
                        .LoadAssetReferenceAsync<GameObject>(this.AssetGUID);
                    if (@ref != null && @ref.IsValid == true)
                    {
                        var result = @ref.@object as GameObject;
                        if(result.GetComponent<T>() != null)
                        {
                            this.LoadedAsset = @ref;
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                return false;
            }
        }
        public async UniTask<T> InstantiateAsync(Transform parent = null)
        {
            if (AddressableAssetsService.IsAvailable == false)
            {
                return null;
            }
            if (this.LoadedAsset?.IsValid == true)
            {
                return this.Instantiate(parent);
            }
            else
            {
                if(await this.PreLoadAsync() == true)
                {
                    return this.Instantiate(parent);
                }
            }
            return null;
        }
        public T Instantiate(Transform parent = null)
        {
            if (AddressableAssetsService.IsAvailable == true)
            {
                if (this.LoadedAsset?.IsValid == true)
                {
                    if(this.IsUsingObjectPool == true)
                    {
                        var val = this.InternalGetPooledObject();
                        if(val != null)
                        {
                            val.transform.parent = parent;
                            val.gameObject.SetActive(false);
                            return val;
                        }
                    }

                    var result = this.LoadedAsset.@object as GameObject;
                    if (result != null)
                    {
                        var obj = GameObject
                               .Instantiate<GameObject>(result as UnityEngine.GameObject, parent);
                        this.LoadedAsset.BindAndRefCountFrom<T>(obj);
                        obj.SetActive(false);
                        return obj.GetComponent<T>();
                    }
                }
            }
            return null;
        }
        public void Dispose()
        {
            this.Dispose(disposing: true);
            //GC.SuppressFinalize(this);
        }
        public T CreateInstance(Transform parent = null)
        {
            if (this.IsUsingObjectPool == true)
            {
                var inst = this.Instantiate(parent);
                if (inst != null)
                {
                    this.ObjectPoolActive.Add(inst);
                }

                return inst;
            }
            else
            {
                return this.Instantiate(parent);
            }
        }
        public bool Release(T @object)
        {
            if (this.IsUsingObjectPool == true && @object != null)
            {
                this.ReleasePooledObject(@object);
                return true;
            }

            return false;
        }
    }
    //!<=================================================================================
}
