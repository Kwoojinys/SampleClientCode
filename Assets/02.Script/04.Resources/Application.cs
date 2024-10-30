using System;
using System.Runtime.CompilerServices;
using SCC.UTIL;

namespace SCC.Foundation
{
    //!<=================================================================================
    //IApplication
    //!<=================================================================================
    public interface IApplication
    {
        bool IsAudioPause    { get; set; }
        int ScreenWidth      { get; }
        int ScreenHeight     { get; }
        UnityEngine.ScreenOrientation ScreenOrientation { get; }

        UnityEngine.Rect ScreenSafeArea { get; }
        static bool IsAvailable  { get; protected set; }

        event System.Action OnUpdateScreenSafeArea;
        event System.Action OnUpdateApplicationQuit;
        event System.Action<bool> OnUpdateApplicationPause;
        event System.Action OnUpdateApplicationReactivate;

        void OnUpateCameraFOV();
        void OnFireApplicationQuit();
        void OnUnloadScene(string current);
    }
    //!<=================================================================================
    //Application
    //!<=================================================================================

    public class Application : IApplication
    {
        private static Application _DefaultFoundation = null;

        internal static IApplication ActiveFoundation
        {
            get
            {
                if(Application.OverrideFoundation != null)
                {
                    return Application.OverrideFoundation;
                }
                else
                {
                    Application._DefaultFoundation ??= new Application();

                    return Application._DefaultFoundation;
                }
            }
        }

        internal static IApplication _OverrideFoundation;

        public static IApplication Instance => Application.ActiveFoundation;
        public static IApplication OverrideFoundation
        {
            get => Application._OverrideFoundation;
            set 
            {
                if (Application._DefaultFoundation != null)
                {
                    Application._DefaultFoundation = null;
                }
                else
                {
                    Application._OverrideFoundation = value;
                }
            }
        }
        public static bool IsAvailable => IApplication.IsAvailable;
        public static bool IsPlaying => UnityEngine.Application.isPlaying;

        public bool IsAudioPause
        {
            get => UnityEngine.AudioListener.pause;
            set => UnityEngine.AudioListener.pause = value;
        }
        public int ScreenWidth  => UnityEngine.Screen.width;
        public int ScreenHeight => UnityEngine.Screen.height;
        public UnityEngine.ScreenOrientation ScreenOrientation => UnityEngine.Screen.orientation;

        public UnityEngine.Rect ScreenSafeArea => UnityEngine.Screen.safeArea;

        //!<=============================================================================
        public Application()
        {
            IApplication.IsAvailable = true;
        }
        public static void OnFireApplicationQuit()
        {
            if(Application.IsAvailable == true)
            {
                Application.Instance.OnFireApplicationQuit();
            }
        }
        public void OnUnloadScene(string current)
        {
            SCC.Foundation.InternalResources
                .UnloadUnusedAssets();
            SCC.Foundation.Resources.Instance?
                .DoCleanupCurrentScene(current);
        }
        public static void OnUpateCameraFOV()
        {
            if (Application.IsAvailable == true)
            {
                Application.Instance.OnUpateCameraFOV();
            }
        }
        void IApplication.OnUpateCameraFOV()
        {

        }
        void IApplication.OnFireApplicationQuit()
        {
            UnityEngine.Application.Quit();
        }
        event Action IApplication.OnUpdateScreenSafeArea
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event Action IApplication.OnUpdateApplicationQuit
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event Action<bool> IApplication.OnUpdateApplicationPause
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event Action IApplication.OnUpdateApplicationReactivate
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }
    }

    //!<=================================================================================
}
