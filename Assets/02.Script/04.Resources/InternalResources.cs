using UnityEngine;

namespace SCC.Foundation
{
    //!<===============================================================================
    //BuiltInResources
    //!<===============================================================================

    public static class InternalResources
    {
        //!<============================================================================
        //Load
        //!<============================================================================
        public static T Load<T>(string path) where T : UnityEngine.Object
        {
            return UnityEngine.Resources.Load<T>(path);
        }

        //!<============================================================================
        //Instantiate
        //!<============================================================================
        public static T Instantiate<T>(string path, Transform parent) where T : UnityEngine.Object
        {
            var prefab = InternalResources.Load<T>(path);
            return prefab == null ? null : 
                UnityEngine.GameObject.Instantiate<T>(prefab, parent);
        }
        //!<============================================================================
        //UnloadUnusedAssets
        //!<============================================================================
        public static void UnloadUnusedAssets()
        {
            UnityEngine.Resources.UnloadUnusedAssets();
        }

    }
}