using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SCC.Foundation
{
    //!<=================================================================================
    //IResources
    //!<=================================================================================

    public interface IResources
    {
        static bool IsAvailable { get; protected set; }
        Sprite  ERROR_SPRITE    { get; }
        void LoadExtraResource<_T>(string addressKey, bool manual, System.Action<_T> callback)
            where _T : UnityEngine.Object;
        UniTask<_T> LoadExtraResourceAsync<_T>(string addressKey, bool manual) where _T : UnityEngine.Object;

        _T LoadInternalResource<_T>(string key)where _T : UnityEngine.Object;

        void ReleaseExtraAsset(string runtimekey);
        void ManualReleaseExtraAsset(UnityEngine.Object source);

        void DoCleanupCurrentScene(string scene);
    }

    //!<=================================================================================
    //Resources
    //!<=================================================================================
    public class Resources : IResources
    {
        private static Resources _DefaultFoundation = null;

        internal static IResources ActiveFoundation
        {
            get
            {
                if (Resources.OverrideFoundation != null)
                {
                    return Resources.OverrideFoundation;
                }
                else
                {
                    Resources._DefaultFoundation ??= new Resources();

                    return Resources._DefaultFoundation;
                }
            }
        }

        internal static IResources _OverrideFoundation;

        public static IResources Instance => Resources.ActiveFoundation;
        public static IResources OverrideFoundation
        {
            get => Resources._OverrideFoundation;
            set
            {
                if (Resources._DefaultFoundation != null)
                {
                    Resources._DefaultFoundation = null;
                }
                else
                {
                    Resources._OverrideFoundation = value;
                }
            }
        }
        public static bool IsAvailable => IResources.IsAvailable;

        public Sprite ERROR_SPRITE => null;

        //!<===============================================================================
        //LoadResourceAsync
        void IResources.LoadExtraResource<_T>(string addressKey, bool manual, System.Action<_T> callback)
        {

        }

        UniTask<_T> IResources.LoadExtraResourceAsync<_T>(string addressKey, bool manual)
        {
            return default;
        }
        //!<===============================================================================
        //LoadInternalResource
        _T IResources.LoadInternalResource<_T>(string key) => default;

        //!<===============================================================================
        //DoCleanup
        public void DoCleanupCurrentScene(string scene)
        {

        }
        //!<===============================================================================
        //ReleaseExtraAsset
        //!<===============================================================================
        public void ReleaseExtraAsset(string runtimekey)
        {

        }
        //!<===============================================================================
        //ManualReleaseExtraAsset
        //!<===============================================================================
        public void ManualReleaseExtraAsset(UnityEngine.Object source)
        {

        }
    }
}
