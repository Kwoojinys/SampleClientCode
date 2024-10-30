using System;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering;

namespace SCC
{
    //!<========================================================================================
    public class AddressableLoadedAssetReferenceBind : MonoBehaviour, 
        IAddressableAssetRuntimeReference
    {
        //!<=====================================================================================

        public string RuntimeKey            { get; protected set; } = null;
        public IAddressableLoadedAsset @Ref { get; protected set; } = null;

        //!<=====================================================================================

        public AddressableLoadedAssetReferenceBind Bind(IAddressableLoadedAsset reference)
        {
            this.@Ref       = reference;
            this.RuntimeKey = reference.Key;
            return this;
        }
        public AddressableLoadedAssetReferenceBind Bind(string runtimeKey)
        {
            this.@Ref       = null;
            this.RuntimeKey = runtimeKey;
            return this;
        }
        private void OnDestroy()
        {
            if(string.IsNullOrEmpty(this.RuntimeKey) == false && 
                SCC.Application.IsPlaying == true)
            {
                if (AddressableAssetsService.IsAvailable == true)
                {
                    AddressableAssetsService.Instance
                        .ReleaseAsset(this);
                }
            }
            this.@Ref       = null;
            this.RuntimeKey = null;
        }
    }
}
