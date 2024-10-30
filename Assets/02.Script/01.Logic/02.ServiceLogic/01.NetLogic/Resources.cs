using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SCC.Foundation;
using SCC.UTIL;
using UnityEngine;

namespace SCC
{
    //!<===============================================================================
    //Resources
    //!<===============================================================================
    [AddComponentMenu("")]
    public class Resources : SCC.SingletonBehaviour<Resources>, IResources
    {
        //!<===========================================================================
        public enum LogicState
        {
            Uninitialized = 0,
            Initializing,
            Initialized,
            InitializedError,
        }

        //!<===========================================================================

        protected PreLoadedInternalResourcesData PreLoadResources;

        //!<===========================================================================

        public UnityEngine.Material UIGrayMaterial  => this.PreLoadResources?.UIMaterialGray ?? null;
        public UnityEngine.Material UIColorFill     => this.PreLoadResources?.UIMaterialColorFill ?? null;
        public Sprite PossessionSpriteGold          => this.PreLoadResources?.PossessionGold ?? null;
        public Sprite ERROR_SPRITE                  => this.PreLoadResources.ErrorIcon ?? null;
        public LogicState State { get; protected set; } = LogicState.Uninitialized;
        public bool IsRun => this.State == LogicState.Initialized;

        //!<===========================================================================

        protected ResourcesCashContainer<UnityEngine.Sprite> SpriteResources = null;

        //!<===============================================================================
        //!<===============================================================================
        //CheckInit
        //!<===============================================================================
        public override void CheckInit()
        {
            this.Init();
        }
        //!<===============================================================================
        //Init
        //!<===============================================================================
        protected override void Init()
        {
            if(this.State == LogicState.Uninitialized)
            {
                this.State = LogicState.Initializing;

                base.Init();

                this.SpriteResources = new ResourcesCashContainer<Sprite>(this);

                SCC.Foundation.Resources.OverrideFoundation = SCC.Resources.Instance;

                AddressableAssetsService.Instance.CheckInit();

                if (this.PreLoadResources == null)
                {
                    this.PreLoadResources =
                        SCC.Foundation.InternalResources
                        .Load<PreLoadedInternalResourcesData>("PreLoadedResourcesData");
                }

                UnityEngine.Application.lowMemory += this.InternalOnApplicationLowMemory;
                this.State = LogicState.Initialized;
            }
        }
        //!<===============================================================================
        //OnBeforeDestroy
        //!<===============================================================================
        protected override void OnBeforeDestroy()
        {
            if(this.State == LogicState.Initialized)
            {
                UnityEngine.Application.lowMemory -= this.InternalOnApplicationLowMemory;
            }
        }
        //!<===============================================================================
        //LoadExtraResourceAsync
        //!<===============================================================================
        public void LoadExtraResource<_T>(string addressKey, bool manual, System.Action<_T> callback)
            where _T : UnityEngine.Object
        {
            if (this.State == LogicState.Initialized)
            {
                AddressableAssetsService.GetSafeService(i =>
                {
                    i.LoadResource<_T>(addressKey, manual, callback);
                });
            }
        }
        public async UniTask<_T> LoadExtraResourceAsync<_T>(string addressKey, bool manual)
             where _T : UnityEngine.Object
        {
            if (this.State == LogicState.Initialized)
            {
                var inst = await AddressableAssetsService.GetSafeServiceAsync();
                var @obj = await inst.LoadAssetAsync<_T>(addressKey, manual);

                return @obj;
            }

            return default;
        }
        public async UniTask<IAddressableLoadedAsset> LoadExtraAssetReferenceAsync<_T>(string addressKey)
             where _T : UnityEngine.Object
        {
            if (this.State == LogicState.Initialized)
            {
                var inst        = await AddressableAssetsService.GetSafeServiceAsync();
                var reference   = await inst.LoadAssetReferenceAsync<_T>(addressKey);

                return reference;
            }

            return default;
        }
        public void LoadExtraAssetReference<_T>(string addressKey,System.Action<IAddressableLoadedAsset> callback)
             where _T : UnityEngine.Object
        {
            if (this.State == LogicState.Initialized)
            {
                AddressableAssetsService.GetSafeService(i =>
                {
                    i.LoadResource<_T>(addressKey, callback);
                });
            }
        }
        //!<===============================================================================
        //ReleaseExtraAsset
        //!<===============================================================================
        public void ReleaseExtraAsset(string runtimekey)
        {
            if (this.State == LogicState.Initialized)
            {
                if (AddressableAssetsService.IsAvailable == true)
                {
                    AddressableAssetsService.Instance.ReleaseAsset(runtimekey);
                }
            }
        }
        //!<===============================================================================
        //ManualReleaseExtraAsset
        //!<===============================================================================
        public void ManualReleaseExtraAsset(UnityEngine.Object source)
        {
            if (this.State == LogicState.Initialized)
            {
                if (AddressableAssetsService.IsAvailable == true)
                {
                    AddressableAssetsService.Instance.ManualReleaseAsset(source);
                }
            }
        }
        //!<===============================================================================
        //LoadInternalResource
        //!<===============================================================================
        public _T LoadInternalResource<_T>(string key)
           where _T : UnityEngine.Object => InternalResources.Load<_T>(key);

        //!<===============================================================================
        //InternalOnApplicationLowMemory
        //!<===============================================================================
        protected void InternalOnApplicationLowMemory()
        {

        }

        //!<===============================================================================
        //DoCleanupCurrentScene
        //!<===============================================================================
        public void DoCleanupCurrentScene(string scene)
        {
            this.SpriteResources?.DoCleanUp();

            if(AddressableAssetsService.HasInstance == true)
            {
                AddressableAssetsService.Instance.ReleaseAllAsset();
            }

            InternalResources.UnloadUnusedAssets();
        }


        //!<===============================================================================
        //InternalGetSpriteExtraAsyncResources
        //!<===============================================================================
        public async UniTask<Sprite> LoadSpriteExtraResourcesAsync(string address)
        {
            Sprite sprite = null;
            if (this.IsRun == true)
            {
                sprite = await this.SpriteResources.TryGetExtraAsync(address);
                if (sprite == null){
                    sprite = this.ERROR_SPRITE;
                }
            }
            return sprite;
        }
        //!<===============================================================================
        //InternalGetSpriteInternalResources
        //!<===============================================================================
        public Sprite LoadSpriteInternalResources(string path)
        {
            Sprite sprite = null;
            if (this.IsRun == true)
            {
                sprite = this.SpriteResources.TryGetInternal(path);
                if (sprite == null){
                    sprite = this.ERROR_SPRITE;
                }
            }
            return sprite;
        }
        //!<===============================================================================
        //InternalGetSpriteInternalResources
        //!<===============================================================================
    }

}
