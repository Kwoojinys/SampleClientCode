using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.ResourceProviders;
using Cysharp.Threading.Tasks;
using UnityEngine.UI.Extensions;

namespace SCC
{
    //!<=================================================================================
    //AddressableAsset
    //!<=================================================================================
    public interface IAddressableLoadedAsset
    {
        string Key                       { get; }
        System.Type Type                 { get; }
        UnityEngine.Object @object       { get; }
        System.DateTime LastAccessTime   { get; }
        int RefRequest                   { get; }
        bool IsValid                     { get; }
    }

    //!<========================================================================================
    //IAddressableAssetRuntimeReference
    //!<========================================================================================
    public interface IAddressableAssetRuntimeReference
    {
        public string RuntimeKey { get; }
    }
    //!<========================================================================================
    //AddressableAssetsServiceExtension
    //!<========================================================================================
    public static class AddressableAssetsServiceExtension
    {

        //!<=====================================================================================
        //BindOnlyFrom
        //!<=====================================================================================
        public static _T BindOnlyFrom<_T>(this IAddressableLoadedAsset reference, GameObject @object)
            where _T : UnityEngine.Component
        {
            return @object.GetOrAddComponent<AddressableLoadedAssetReferenceBind>()
                .Bind(reference)
                .GetComponent<_T>();
        }

        //!<=====================================================================================
        //BindAndRefCountFrom
        //!<=====================================================================================
        public static _T BindAndRefCountFrom<_T>(this IAddressableLoadedAsset reference, GameObject @object)
            where _T : UnityEngine.Component
        {
            if (AddressableAssetsService.IsAvailable == false)
            {
                return null;
            }
            if (reference.IsValid == false)
            {
                Debug.LogError($"ERROR BindToReference=[{reference.Key}]");
                return null;
            }

            var inst = AddressableAssetsService.Instance;
            var @ref = inst.IncrementReferenceCount(reference);
            return @object.GetOrAddComponent<AddressableLoadedAssetReferenceBind>()
                .Bind(@ref)
                .GetComponent<_T>();
        }
        public static _T CreateObjectAndBind<_T>(this IAddressableLoadedAsset reference,
            UnityEngine.Transform parent = null)
           where _T : UnityEngine.Component
        {
            if (reference.IsValid == false)
            {
                Debug.LogError($"ERROR BindToReference=[{reference.Key}]");
                return null;
            }

            if (reference.@object is UnityEngine.GameObject prefab)
            {
                var @object = UnityEngine.GameObject.Instantiate<GameObject>(prefab, parent);
                return @object.GetOrAddComponent<AddressableLoadedAssetReferenceBind>()
                    .Bind(reference)
                    .GetComponent<_T>();
            }
            return null;
        }

        //!<=====================================================================================
        //BindAndRefCountFrom
        //!<=====================================================================================
        public static IAddressableLoadedAsset BindAndRefCountFrom<_T>(this AssetReference reference,GameObject @object)
             where _T : UnityEngine.Component
        {
            if(AddressableAssetsService.IsAvailable == false)
            {
                return null;
            }
            if(reference.IsDone == false || reference.IsValid() == false)
            {
                Debug.LogError($"ERROR BindToReference=[{reference.AssetGUID}]");

                return null;
            }
            var inst = AddressableAssetsService.Instance;
            var @ref = inst.HasLoadedAsset<_T>(reference.AssetGUID) == true
                ? inst.IncrementReferenceCount<_T>(reference)
                : inst.AppendReferenceCacheAsset<_T>(reference);
            if (@ref?.IsValid == true)
            {
                var comp = @object.GetOrAddComponent<AddressableLoadedAssetReferenceBind>()
                    .Bind(@ref);
                return @ref;
            }
            else
            {
                Debug.LogError($"ERROR @ref Invalid=[{reference.AssetGUID}]");
            }

            return null;
        }
    }

    //!<==========================================================================================
    [AddComponentMenu("")]
    public class AddressableAssetsService : SCC.SingletonBehaviour<AddressableAssetsService>
    {
        //!<=================================================================================
        //ServicesState
        //!<=================================================================================
        public enum ServicesState
        {
            Uninitialized = 0,
            Initializing,
            Initialized,
            InitializedError,
            Destroyed,
        }

        //!<=================================================================================

        public static bool IsAvailable => AddressableAssetsService.HasInstance == true &&
            AddressableAssetsService.Instance.LogicState == ServicesState.Initialized;

        //!<=================================================================================
        //GetSafeService
        //!<=================================================================================
        public static async UniTask<AddressableAssetsService> GetSafeServiceAsync()
        {
            var inst = AddressableAssetsService.Instance;
            if (inst != null)
            {
                var load = false;
                if (inst.LogicState == ServicesState.Initialized)
                {
                    return inst;
                }
                else
                {
                    if (inst.LogicState == ServicesState.Initializing)
                    {
                        inst.InitCallbacks.Add(i => load = true);
                    }
                    else
                    {
                        if (inst.LogicState == ServicesState.Uninitialized)
                        {
                            inst.Init();
                            inst.InitCallbacks.Add(i => load = true);
                        }
                        else
                        {
                            Debug.LogError("AddressableAssetsService CurrState = ERROR");
                        }
                    }

                    await UniTask.WaitUntil(() => load);

                    return inst;
                }
            }

            return null;
        }
        public static void GetSafeService(System.Action<AddressableAssetsService> callback)
        {
            var inst = AddressableAssetsService.Instance;

            // initiailzed 되지 않은 경우 콜백에 등록해놓고 initialized 될때를 기다림..

            if (inst == null)
            {
                callback?.Invoke(null);
            }
            else
            {
                if(inst.LogicState == ServicesState.Initialized)
                {
                    callback?.Invoke(inst);
                }
                else if (inst.LogicState == ServicesState.Initializing)
                {
                    inst.InitCallbacks.Add(callback);
                }
                else
                {
                    if (inst.LogicState == ServicesState.Uninitialized)
                    {
                        inst.Init();
                        inst.InitCallbacks.Add(callback);
                    }
                    else
                    {
                        Debug.LogError("AddressableAssetsService CurrState = ERROR");
                    }
                }
            }
        }
        
        //!<=================================================================================
        //AddressableLoadedAsset
        //!<=================================================================================
        public class AddressableLoadedAsset : IAddressableLoadedAsset, SCC.UTIL.IKeyHintT<string>,SCC.UTIL.IKeyHint
        {
            //!<=================================================================================

            public string Key                   { get; protected set; }
            public System.Type          Type    { get; protected set; }
            public UnityEngine.Object   @object { get; protected set; }
            public System.DateTime      LastAccessTime  { get; protected set; }
            public int RefRequest                       { get; protected set; } = 0;
            public AsyncOperationHandle Handle          { get; protected set; }
            string SCC.UTIL.IKeyHintT<string>._key => this.Key;
            int SCC.UTIL.IKeyHint._key => this.@object.GetHashCode();

            protected List<string> IncrementStackTrace = new() { Capacity = 50, };

            public bool IsValid =>
                string.IsNullOrEmpty(this.Key) == false && this.@object != null;
            public bool IsCompareT(Type type) 
            {
                return this.@object != null ? this.Type == type : false;
            }
            public AddressableLoadedAsset Clear()
            {
                this.Key     = null;
                this.Type    = null;
                this.@object = null;
                this.RefRequest = 0;
                this.LastAccessTime = System.DateTime.MinValue;
                this.Handle         = default(AsyncOperationHandle);

                this.IncrementStackTrace.Clear();
                return this;
            }
            public AddressableLoadedAsset Alloc(
                string key, System.Type type, AsyncOperationHandle handle, int refcount = 1)
            {
                this.Key        = key;
                this.Type       = type;
                this.@object    = handle.Result as UnityEngine.Object;
                this.Handle     = handle;
                this.LastAccessTime = TimeManager.Now;
                this.RefRequest     = refcount;
                return this;
            }
            public AddressableLoadedAsset AllocObject(
                string key, System.Type type, UnityEngine.Object @object,int refcount = 1)
            {
                this.Key        = key;
                this.Type       = type;
                this.@object    = @object;
                this.Handle     = default(AsyncOperationHandle);
                this.LastAccessTime = TimeManager.Now;
                this.RefRequest     = refcount;
                return this;
            }
            //IncrementReferenceCount
            public AddressableLoadedAsset Ref()
            {
                this.RefRequest++;
                this.LastAccessTime = TimeManager.Now;

                this.IncrementStackTrace.Add(UnityEngine.StackTraceUtility.ExtractStackTrace());
                return this;
            }
            //DecrementReferenceCount
            public AddressableLoadedAsset UnRef()
            {
                this.RefRequest--;
                return this;
            }
        }

        //!<=================================================================================

        public ServicesState LogicState { get; protected set; } = ServicesState.Uninitialized;
        public bool IsRun => this.LogicState == ServicesState.Initialized;

        //!<=================================================================================

        protected SCC.UTIL.IndexListT<string, AddressableLoadedAsset> CacheAsset = new(100);
        protected HashSet<string>           ErrorAssets                         = new();
        protected Queue<AddressableLoadedAsset>   LoadedAssetInfoPools          = new();
        protected List<System.Action<AddressableAssetsService>> InitCallbacks   = new();
        protected HashSet<string> TryLoadAssetNames = new ();

        //!<=================================================================================
        //CheckInit
        //!<=================================================================================
        public override void CheckInit()
        {
            this.Init();
        }
        //!<=================================================================================
        //OnBeforeDestroy
        //!<=================================================================================
        protected override void OnBeforeDestroy()
        {
            this.LogicState = ServicesState.Destroyed;
            this.ReleaseAllAsset();
        }
        //!<=================================================================================
        //Init
        //!<=================================================================================
        protected override void Init()
        {
            this.OnFireInitialized();
        }
        //!<=================================================================================
        //OnFireInitialized
        //!<=================================================================================
        public void OnFireInitialized()
        {
            if(this.LogicState == ServicesState.Uninitialized)
            {
                UnityEngine.Application.backgroundLoadingPriority = ThreadPriority.High;
                this.LogicState = ServicesState.Initializing;

                this.CacheAsset.Clear();
                this.LoadedAssetInfoPools.Clear();

                Addressables.InitializeAsync(false).Completed += i =>
                {
                    if (i.IsDone == true && i.Status == AsyncOperationStatus.Succeeded)
                    {
                        this.LogicState = ServicesState.Initialized;

                        Debug.Log("<Color=green><b>Addressable Init Done.</b></Color>");

                        if (this.InitCallbacks.Count > 0)
                        {
                            this.InitCallbacks.ForEach(i => i?.Invoke(this));
                            this.InitCallbacks.Clear();
                        }
                    }
                    else
                    {
                        this.LogicState = ServicesState.InitializedError;

                        if (i.OperationException.Data != null)
                        {
                            Debug.LogError($"Addressable Init Failed! " +
                                            $"msg = [{i.OperationException.Message}] " +
                                            $"stacktrace : [{i.OperationException.StackTrace}]");
                        }
                        else
                        {
                            Debug.LogError($"Addressable Init Failed by Unknown Error.");
                        }
                    }
                };
            }
        }
        //!<=================================================================================
        //InternalPopAsset
        //!<=================================================================================
        protected AddressableLoadedAsset InternalPopAsset()
        {
            if(this.LoadedAssetInfoPools.Count > 0)
            {
                return this.LoadedAssetInfoPools.Dequeue();
            }
            else
            {
                return new AddressableLoadedAsset();
            }
        }
        //!<=================================================================================
        //ReleaseAsset
        //!<=================================================================================
        public void ReleaseAsset(string runtimekey)
        {
            if (this.LogicState == ServicesState.Initialized)
            {
                if(this.CacheAsset.Count <= 0)
                {
                    return;
                }

                var asset = this.CacheAsset.GetValue(runtimekey);
                if(asset?.IsValid == true)
                {
                    var ref_count = asset.UnRef().RefRequest;
                    if (ref_count <= 0)
                    {
                        if(asset.Handle.IsValid() == true)
                        {
                            Addressables.Release(asset.Handle);
                        }
                        else if(asset.@object != null)
                        {
                            Addressables.Release(asset.@object);
                        }

                        this.CacheAsset.Delete(asset.Key);
                        this.LoadedAssetInfoPools.Enqueue(asset.Clear());
                    }  
                }
                else
                {
                    Debug.LogError($"ERROR ReleaseAsset=[{runtimekey}]");
                }
            }
        }

        //!<=================================================================================
        //ManualReleaseAsset
        //!<=================================================================================
        public void ManualReleaseAsset(UnityEngine.Object source)
        {
            if (this.LogicState == ServicesState.Initialized)
            {
                Addressables.Release(source);
            }
        }
        //!<=================================================================================
        //ReleaseAsset
        //!<=================================================================================
        public void ReleaseAsset(IAddressableAssetRuntimeReference reference)
        {
            if (this.LogicState == ServicesState.Initialized)
            {
                this.ReleaseAsset(reference.RuntimeKey);
            }
        }
        //!<=================================================================================
        //ReleaseAsset
        //!<=================================================================================
        public void ReleaseAllAsset()
        {
            if (this.LogicState == ServicesState.Initialized)
            {
                var cache = this.CacheAsset.Items;
                for(var i = 0; i < cache.Count; ++i)
                {
                    var asset = cache[i];
                    if(asset.IsValid == true)
                    {
                        if (asset.Handle.IsValid() == true)
                        {
                            Addressables.Release(asset.Handle);
                        }
                        else if (asset.@object != null)
                        {
                            Addressables.Release(asset.@object);
                        }
                    }

                    this.LoadedAssetInfoPools
                        .Enqueue(asset.Clear());
                }

                this.CacheAsset.Clear();
            }
        }
        //!<=================================================================================
        //HasLoadedAsset
        //!<=================================================================================
        public bool HasLoadedAsset<_T>(string name) where _T : UnityEngine.Object
        {
            if (this.LogicState == ServicesState.Initialized)
            {
                var loadedAsset = this.CacheAsset.GetValue(name);
                if (loadedAsset != null && loadedAsset.IsCompareT(typeof(_T)) == true)
                {
                    return true;
                }
            }

            return false;
        }
        public bool DecrementReferenceCount(IAddressableLoadedAsset @ref)
        {
            if (@ref == null)
            {
                return false;
            }

            if (this.LogicState == ServicesState.Initialized)
            {
                if (@ref.IsValid == false)
                {
                    Debug.LogError($"ERROR AssetReferenceInValid={@ref.Key}");
                    return false;
                }

                var loadedAsset = this.CacheAsset.GetValue(@ref.Key);
                if (loadedAsset != null && loadedAsset == @ref)
                {
                    loadedAsset.UnRef();
                    return true;
                }
            }
            return false;
        }
        public IAddressableLoadedAsset IncrementReferenceCount(IAddressableLoadedAsset @ref)
        {
            if(@ref == null)
            {
                return null;
            }

            if (this.LogicState == ServicesState.Initialized)
            {
                if (@ref.IsValid == false)
                {
                    Debug.LogError($"ERROR AssetReferenceInValid={@ref.Key}");
                    return null;
                }

                var loadedAsset = this.CacheAsset.GetValue(@ref.Key);
                if (loadedAsset != null && loadedAsset == @ref)
                {
                    
                    return loadedAsset.Ref();
                }
            }
            return null;
        }
        //!<=================================================================================
        //IncrementReferenceCount
        //!<=================================================================================
        public IAddressableLoadedAsset IncrementReferenceCount<_T>(AssetReference @ref) where _T : UnityEngine.Object
        {
            if (this.LogicState == ServicesState.Initialized)
            {
                if (@ref.OperationHandle.IsValid() == false ||
                   @ref.OperationHandle.IsDone == false)
                {
                    Debug.LogError($"ERROR AssetReferenceInValid={@ref.AssetGUID}");
                    return null;
                }

                var loadedAsset = this.CacheAsset.GetValue(@ref.AssetGUID);
                if (loadedAsset != null && loadedAsset.IsCompareT(typeof(_T)) == true)
                {
                    return loadedAsset.Ref();
                }
            }

            return null;
        }
        //!<=================================================================================
        //AppendReferenceCacheAsset
        //!<=================================================================================
        public IAddressableLoadedAsset AppendReferenceCacheAsset<_T>(AssetReference @ref) where _T : UnityEngine.Object
        {
            if (this.LogicState == ServicesState.Initialized)
            {
                if(@ref.OperationHandle.IsValid() == false ||
                    @ref.OperationHandle.IsDone == false)
                {
                    Debug.LogError($"ERROR AssetReferenceInValid={@ref.AssetGUID}");
                    return null;
                }

                var loadedAsset = this.CacheAsset.GetValue(@ref.AssetGUID);
                if(loadedAsset == null)
                {
                    loadedAsset = this.InternalPopAsset()
                         .Clear()
                         .Alloc(@ref.AssetGUID, typeof(_T), @ref.OperationHandle);

                    if (this.CacheAsset.Add(loadedAsset) == true)
                    {
                        return loadedAsset;
                    }
                    else
                    {
                        Debug.LogError($"ERROR InsertCache={@ref.AssetGUID}");
                    }
                }
                else
                {
                    Debug.LogError($"ERROR AlreadyCache={@ref.AssetGUID}");
                }
            }
            return null;
        }
        //!<=================================================================================
        //LoadResourceAsyncCall
        //!<=================================================================================
        public void LoadResource<_T>(string addressKey, bool manual, System.Action<_T> callback) 
            where _T : UnityEngine.Object
        {
            if (this.LogicState == ServicesState.Initialized)
            {
                _ = this.InternalLoadAssetAsync(addressKey, manual, callback);
            }
            else
            {
                callback?.Invoke(default);
            }
        }
        public void LoadResource<_T>(string addressKey,System.Action<IAddressableLoadedAsset> callback)
            where _T : UnityEngine.Object
        {
            if (this.LogicState == ServicesState.Initialized)
            {
                this.InternalLoadAssetRefAsync<_T>(addressKey, callback)
                    .Forget();
            }
            else
            {
                callback?.Invoke(default);
            }
        }

        //!<=================================================================================
        //InternalLoadAssetAsync
        //!<=================================================================================
        protected async UniTaskVoid InternalLoadAssetAsync<_T>(string addressKey, bool manual, System.Action<_T> callback)
            where _T : UnityEngine.Object
        {
            var resource = await this.LoadAssetAsync<_T>(addressKey, manual);
            callback?
                .Invoke(resource);
        }
        protected async UniTaskVoid InternalLoadAssetRefAsync<_T>(string addressKey,System.Action<IAddressableLoadedAsset> callback)
            where _T : UnityEngine.Object
        {
            var @ref = await this.LoadAssetReferenceAsync<_T>(addressKey);
            callback?
                .Invoke(@ref);
        }
        //!<=================================================================================
        //PreLoadAsset
        //!<=================================================================================
        public async UniTaskVoid PreLoadAsset<_T>(string addressKey)
           where _T : UnityEngine.Object
        {
            try
            {

                if (this.ErrorAssets.Contains(addressKey) == true ||
                    this.TryLoadAssetNames.Contains(addressKey) == true||
                    this.CacheAsset.GetValue(addressKey) != null)
                {
                    return;
                }

                this.TryLoadAssetNames.Add(addressKey);
                var handle = Addressables.LoadAssetAsync<_T>(addressKey);
                var result = await handle.ToUniTask();
                if (result != null)
                {
                    var loadedAsset = this.InternalPopAsset()
                          .Clear()
                          .Alloc(addressKey, result.GetType(), handle,0);

                    if (this.TryLoadAssetNames.Remove(addressKey) == false)
                    {
                        Debug.LogError($"LoadAssetReferenceAsync TryLoadAssetNames ={addressKey}");
                    }
                    if (this.CacheAsset.Add(loadedAsset) == false)
                    {
                        Debug.LogError($"LoadAssetReferenceAsync Already Cache={addressKey}");
                    }
                }
                else
                {
                    this.ErrorAssets
                        .Add(addressKey);
                }
            }
            catch (Exception ex)/* when (ex is not OperationCanceledException)*/
            {
                Debug.LogError(ex.Message);
            }
        }
        public async UniTask<int> PreLoadAsset<T>(IReadOnlyList<string> addressKey,string namePrefix) 
            where T : UnityEngine.Object
        {
            try
            {

                //var locToKeys = new Dictionary<string, List<object>>();
                //foreach (IResourceLocator locator in Addressables.ResourceLocators)
                //{
                //    ResourceLocationMap map = locator as ResourceLocationMap;
                //    if (map == null)
                //        continue;
                //    foreach (KeyValuePair<object, IList<IResourceLocation>> keyToLocs in map.Locations)
                //    {
                //        foreach (IResourceLocation loc in keyToLocs.Value)
                //        {
                //            if (!locToKeys.ContainsKey(loc.InternalId))
                //                locToKeys.Add(loc.InternalId, new List<object>() { keyToLocs.Key });
                //            else
                //                locToKeys[loc.InternalId].Add(keyToLocs.Key);
                //        }
                //    }
                //}
                var array = new List<string>() { Capacity = addressKey.Count, };
                for (var i = 0; i < addressKey.Count; ++i)
                {
                    var key = addressKey[i];
                    if (this.ErrorAssets.Contains(key) == false &&
                        this.TryLoadAssetNames.Contains(key) == false &&
                        this.CacheAsset.GetValue(key) == null)
                    {
                        array.Add(key);
                        this.TryLoadAssetNames.Add(key);
                    }
                }

                if(array.Count <= 0)
                {
                    return 0;
                }
                var now = System.DateTime.Now;
                var handle = await Addressables.LoadAssetsAsync<T>(array, i =>
                {
                    var addressKey = string.Format(namePrefix, i.name);
                    if (i != null)
                    {
                        
                        var load = this.InternalPopAsset()
                              .Clear()
                              .AllocObject(addressKey, i.GetType(), i,0);

                        if (this.TryLoadAssetNames.Remove(addressKey) == false)
                        {
                            Debug.LogError($"LoadAssetReferenceAsync TryLoadAssetNames ={addressKey}");
                        }
                        if (this.CacheAsset.Add(load) == false)
                        {
                            Debug.LogError($"LoadAssetReferenceAsync Already Cache={addressKey}");
                        }
                    }
                    else
                    {
                        this.ErrorAssets.Add(addressKey);
                    }
                }, Addressables.MergeMode.Union, false);

                Debug.Log($"================== END [{(System.DateTime.Now - now).TotalMilliseconds}]");
                return handle?.Count ?? 0;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                return 0;
            }
            //finally
            //{
                
            //}
        }
        //!<=================================================================================
        //LoadAssetReferenceAsync
        //!<=================================================================================
        public async UniTask<IAddressableLoadedAsset> LoadAssetReferenceAsync<_T>(string addressKey)
             where _T : UnityEngine.Object
        {
            try
            {
                if (this.LogicState != ServicesState.Initialized ||
                    this.ErrorAssets.Contains(addressKey) == true)
                {
                    return default;
                }
                if (this.TryLoadAssetNames.Contains(addressKey) == true)
                {
                    await UniTask.WaitUntil(() => this.TryLoadAssetNames.Contains(addressKey) == false);
                }

                var loadedAsset = this.CacheAsset.GetValue(addressKey);
                if (loadedAsset?.IsValid == true)
                {
                    if(loadedAsset.IsCompareT(typeof(_T)) == true)
                    {
                        return loadedAsset.Ref();
                    }
                    else
                    {
                        Debug.LogError($"ERROR {typeof(_T)}");
                        return null;
                    }
                }
                this.TryLoadAssetNames.Add(addressKey);
                var handle =  Addressables.LoadAssetAsync<_T>(addressKey);
                var result = await handle.ToUniTask();
                if (result != null)
                {
                    loadedAsset = this.InternalPopAsset()
                          .Clear()
                          .Alloc(addressKey, result.GetType(), handle);

                    if(this.TryLoadAssetNames.Remove(addressKey) == false)
                    {
                        Debug.LogError($"LoadAssetReferenceAsync TryLoadAssetNames ={addressKey}");
                    }
                    if (this.CacheAsset.Add(loadedAsset) == false)
                    {
                        Debug.LogError($"LoadAssetReferenceAsync Already Cache={addressKey}");
                    }
                }
                else
                {
                    this.ErrorAssets
                        .Add(addressKey);
                }

                return loadedAsset;
            }
            catch (Exception ex)/* when (ex is not OperationCanceledException)*/
            {
                Debug.LogError(ex.Message);

                if(this.ErrorAssets.Contains(addressKey) == false)
                {
                    this.ErrorAssets.Add(addressKey);
                }
                if (this.TryLoadAssetNames.Contains(addressKey) == false)
                {
                    this.TryLoadAssetNames.Add(addressKey);
                }
                return default;
            }
        }
        //!<=================================================================================
        //LoadAssetAsync
        //!<=================================================================================
        public async UniTask<_T> LoadAssetAsync<_T>(string addressKey,bool manual = false)
            where _T : UnityEngine.Object
        {
            try
            {
                if (this.LogicState != ServicesState.Initialized ||
                    this.ErrorAssets.Contains(addressKey) == true)
                {
                    return default;
                }

                if (this.TryLoadAssetNames.Contains(addressKey) == true)
                {
                    await UniTask.WaitUntil(() => this.TryLoadAssetNames.Contains(addressKey) == false);
                }

                if (manual == false)
                {
                    var loadedAsset = this.CacheAsset.GetValue(addressKey);
                    if (loadedAsset?.IsValid == true)
                    {
                        if(loadedAsset.IsCompareT(typeof(_T)) == true)
                        {
                            return loadedAsset.Ref().@object as _T;
                        }
                        else
                        {
                            Debug.LogError($"ERROR {typeof(_T)}");
                            return null;
                        }   
                    }
                }

                this.TryLoadAssetNames.Add(addressKey);
                var handle = Addressables.LoadAssetAsync<_T>(addressKey);
                var result = await handle.ToUniTask();
                if (result != null)
                {
                    if(manual == false)
                    {
                        var loadedAsset = this.InternalPopAsset()
                            .Clear()
                            .Alloc(addressKey, result.GetType(), handle);

                        if (this.TryLoadAssetNames.Remove(addressKey) == false)
                        {
                            Debug.LogError($"LoadAssetReferenceAsync TryLoadAssetNames ={addressKey}");
                        }
                        if (this.CacheAsset.Add(loadedAsset) == false)
                        {
                            Debug.LogError($"LoadAssetReferenceAsync Already Cache={addressKey}");
                        }
                    }
                    else
                    {
                        if (this.TryLoadAssetNames.Remove(addressKey) == false)
                        {
                            Debug.LogError($"LoadAssetReferenceAsync TryLoadAssetNames ={addressKey}");
                        }
                    }
                }
                else
                {
                    this.ErrorAssets
                        .Add(addressKey);
                }
                return result;
            }
            catch (Exception ex)/* when (ex is not OperationCanceledException)*/
            {
                Debug.LogError(ex.Message);

                if (this.ErrorAssets.Contains(addressKey) == false)
                {
                    this.ErrorAssets.Add(addressKey);
                }
                if (this.TryLoadAssetNames.Contains(addressKey) == false)
                {
                    this.TryLoadAssetNames.Add(addressKey);
                }
                return default;
            }
        }
        //!<=================================================================================
        //LoadSceneAsyncWithManual
        //!<=================================================================================
        public async UniTask<SceneInstance>LoadSceneAsyncWithManual(string addressKey, 
            LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = false, int priority = 100)
        {
            try
            {
                if (this.LogicState != ServicesState.Initialized )
                {
                    return default;
                }

                var result = await Addressables.LoadSceneAsync(addressKey, loadMode, activateOnLoad, priority);
                return result;
            }
            catch (Exception ex)/* when (ex is not OperationCanceledException)*/
            {
                Debug.LogError(ex.Message);
                return default;
            }
        }
        //!<=================================================================================
        //UnLoadSceneAsync
        //!<=================================================================================
        public async void UnLoadSceneAsync(SceneInstance scene,System.Action<bool> callback)
        {
            try
            {
                if (this.LogicState != ServicesState.Initialized)
                {
                    callback?.Invoke(false);
                    return;
                }

                _ = await Addressables.UnloadSceneAsync(scene,true);

                callback?.Invoke(true);
            }
            catch (Exception ex)/* when (ex is not OperationCanceledException)*/
            {
                Debug.LogError(ex.Message);
                callback?.Invoke(false);
                return;
            }
        }
        //!<=================================================================================
    }

}
