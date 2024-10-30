using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SCC.Foundation;
using SCC.UTIL;
using UnityEngine;

//!<=================================================================================

namespace SCC
{
    //!<==============================================================================
    [AddComponentMenu("")]
    public class Application : SCC.SingletonBehaviour<Application>,SCC.Foundation.IApplication
    {
        //!<===========================================================================
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnFireBeforeSceneLoad()
        {
            SCC.Application.Instance.CheckInit();
            SCC.Foundation.Application.OverrideFoundation = SCC.Application.Instance;

            UnityEngine.Application.quitting -= InternalOnApplicationQuit;
            UnityEngine.Application.quitting += InternalOnApplicationQuit;

            SCC.Resources.Instance.CheckInit();

        }
        protected static void InternalOnApplicationQuit()
        {
            Application.IsApplicationQuit = true;

            if (SCC.Application.Instance != null){
                SCC.Application.Instance.InternalOnProcApplicationQuit();
            }
        }

        //!<===========================================================================

        public static bool IsPlaying => UnityEngine.Application.isPlaying;
        public static bool IsApplicationQuit { get; private set; } = false;

        //!<===========================================================================

        protected EventHandler EventOnUpdateScreenSafeArea          = new();
        protected EventHandler EventOnUpdateApplicationQuit         = new ();
        protected EventHandler EventOnUpdateApplicationReactivate   = new ();
        protected EventHandler<bool> EventOnUpdateApplicationPause  = new();
        
        //!<===========================================================================
        public static bool IsAvailable
        {
            get => Application.HasInstance == true && 
                SCC.Foundation.IApplication.IsAvailable && Application.IsApplicationQuit == false;

            set => SCC.Foundation.IApplication.IsAvailable = value;
        }
        //!<===========================================================================

        public event System.Action OnUpdateScreenSafeArea
        {
            add     => this.EventOnUpdateScreenSafeArea.Handler += value;
            remove  => this.EventOnUpdateScreenSafeArea.Handler -= value;
        }
        public event System.Action OnUpdateApplicationQuit
        {
            add     => this.EventOnUpdateApplicationQuit.Handler += value;
            remove  => this.EventOnUpdateApplicationQuit.Handler -= value;
        }
        public event System.Action<bool> OnUpdateApplicationPause
        {
            add     => this.EventOnUpdateApplicationPause.Handler += value;
            remove  => this.EventOnUpdateApplicationPause.Handler -= value;
        }
        public event System.Action OnUpdateApplicationReactivate
        {
            add     => this.EventOnUpdateApplicationReactivate.Handler += value;
            remove  => this.EventOnUpdateApplicationReactivate.Handler -= value;
        }

        //!<===========================================================================

        public UnityEngine.Rect LastScreenSafeArea                  { get; protected set; }
          = new UnityEngine.Rect(0, 0, 0, 0);

        public UnityEngine.ScreenOrientation LastScreenOrientation  { get; protected set; }
            = UnityEngine.ScreenOrientation.AutoRotation;
        public bool IsAudioPause
        {
            get => UnityEngine.AudioListener.pause;
            set => UnityEngine.AudioListener.pause = value;
        }
        public int ScreenWidth  => UnityEngine.Screen.width;
        public int ScreenHeight => UnityEngine.Screen.height;
        public UnityEngine.ScreenOrientation ScreenOrientation => UnityEngine.Screen.orientation;
        public UnityEngine.Rect ScreenSafeArea => UnityEngine.Screen.safeArea;
        public bool IsInitialized                   { get; protected set; } = false;
        public bool IsDoClenaup                     { get; protected set; } = false;

        //!<===========================================================================

        void IApplication.OnFireApplicationQuit()
        {
            if (SCC.Foundation.IApplication.IsAvailable == true && 
                SCC.Application.IsApplicationQuit == false && 
                this.gameObject.activeInHierarchy == true)
            {
                this.StartCoroutine(this.InternalApplicationQuit());
            }
        }
        protected override void OnBeforeDestroy()
        {
            this.InternalOnProcApplicationQuit();
        }
        protected void InternalOnProcApplicationQuit()
        {
            if(this != null && this.gameObject.activeInHierarchy == true && 
                this.IsDoClenaup == false && this.IsInitialized == true)
            {
                this.EventOnUpdateApplicationQuit.OnFire();
                this.EventOnUpdateApplicationQuit.OnClear();

                this.EventOnUpdateScreenSafeArea.OnClear();
                this.EventOnUpdateApplicationPause.OnClear();
                this.EventOnUpdateApplicationReactivate.OnClear();

                this.IsInitialized  = false;
                this.IsDoClenaup    = true;
                SCC.Application.IsAvailable   = false;
                Application.IsApplicationQuit = true;
            }
        }
        protected IEnumerator InternalApplicationQuit()
        {
            this.InternalOnProcApplicationQuit();

            yield return new WaitForEndOfFrame();

#if UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass javaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject javaActivity = javaClass.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    javaActivity.Call<bool>("moveTaskToBack", true);
                    javaActivity.Call("finish");
                }
            }
#else
            UnityEngine.Application.Quit();
#endif
        }
        private void InternalOnApplicationLowMemory()
        {
            if (SCC.Application.IsAvailable == false)
                return;

            SCC.Foundation.InternalResources.UnloadUnusedAssets();

            Debug.LogWarning($"<color=red>OnApplicationLowMemory max={SystemInfo.systemMemorySize},battery={SystemInfo.batteryLevel}</color>");
        }
        public override void CheckInit()
        {
            if (this.IsInitialized == false)
            {
                SCC.Application.IsAvailable         = true;
                SCC.Application.IsApplicationQuit   = false;
                UnityEngine.Application.runInBackground = false;
                UnityEngine.Application.targetFrameRate = 60;
                UnityEngine.Screen.sleepTimeout = UnityEngine.SleepTimeout.NeverSleep;

                UnityEngine.Application.lowMemory += this.InternalOnApplicationLowMemory;

                this.IsInitialized  = true;
                this.IsDoClenaup    = false;
            }
        }
        public void OnUnloadScene(string current)
        {
            if (SCC.Application.IsAvailable == true)
            {
                SCC.Foundation.InternalResources
                    .UnloadUnusedAssets();
                SCC.Resources.Instance?
                    .DoCleanupCurrentScene(current);

                System.GC.Collect();
            }
        }
        protected override void Init()
        {
            base.Init();

            this.CheckInit();
        }
        public void OnUpateCameraFOV()
        {
            this.InernalOnProcSafeArea(true);
        }
        protected bool InernalOnProcSafeArea(bool force = false)
        {
            if (this.ScreenWidth <= 0 || this.ScreenHeight <= 0)
            {
                return false;
            }

#if UNITY_EDITOR == false
            if (force == false)
            {
                if (this.ScreenSafeArea == this.LastScreenSafeArea &&
                    this.ScreenOrientation == this.LastScreenOrientation)
                {
                    return false;
                }
            }
#endif
            this.LastScreenSafeArea     = this.ScreenSafeArea;
            this.LastScreenOrientation  = this.ScreenOrientation;

            this.EventOnUpdateScreenSafeArea.OnNeedUpdate();

            
            return true;
        }
        protected void LateUpdate()
        {
            this.InernalOnProcSafeArea();

            if (this.EventOnUpdateScreenSafeArea.IsNeedUpdate == true)
            {
                this.EventOnUpdateScreenSafeArea.OnFire();
            }
        }
    }
}
