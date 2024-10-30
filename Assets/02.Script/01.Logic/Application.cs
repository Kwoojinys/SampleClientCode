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

#if DEV_BUILD_VERSION
        protected void Update()
        {
            this.InternalOnUpdateDebugView();
        }
#endif
        //!<==========================================================================
        //DEV_BUILD_VERSION
        //!<==========================================================================
        [System.Diagnostics.Conditional("DEV_BUILD_VERSION")]
        protected void InternalOnUpdateDebugView()
        {
#if DEV_BUILD_VERSION
            if(this.IsRenderDebugInfo == true)
            {
                this.DEV_InternalOnUpdate();
            }
#endif
        }

#if DEV_BUILD_VERSION
        Unity.Profiling.ProfilerRecorder CpuProfileRecorder;
        Unity.Profiling.ProfilerRecorder TotalMemoryRecorder;
        //Unity.Profiling.ProfilerRecorder TextureCountRecorder;
        //Unity.Profiling.ProfilerRecorder TextureMemoryRecorder;
        Unity.Profiling.ProfilerRecorder RenderProfileRecorder;
        Unity.Profiling.ProfilerRecorder GCUsedMemoryRecorder;
        Unity.Profiling.ProfilerRecorder GCReservedMemoryRecorder;
        Unity.Profiling.ProfilerRecorder RenderTextureBatchesRecorder;
        Unity.Profiling.ProfilerRecorder RendersetPassCallsRecorder;

        protected System.Text.StringBuilder DebugInfoStringBuilder = new System.Text.StringBuilder(300);
        protected bool IsInitDebugInfo   = false;
        protected bool IsRenderDebugInfo = true;
        protected float DeltaTime        = 0.0f;
        protected GUIStyle DrawDebugGUIStyle = null;

        protected void DEV_InternalOnUpdate()
        {
            if (this.IsInitDebugInfo == true)
            {
                this.DeltaTime += (UnityEngine.Time.unscaledDeltaTime - this.DeltaTime) * 0.1f;
            }
        }
        public void DEV_OnChangeDebugInfoRenderingState()
        {
            SAR.Application.IS_DEBUG_STATUS_VIEW = SAR.Application.IS_DEBUG_STATUS_VIEW == false;
        }
        protected void DEV_InternalOnUpdateDebugView()
        {
            if (SAR.Application.IS_DEBUG_STATUS_VIEW == true)
            {
                this.DEV_InternalOnLoadDevView();
            }
            else
            {
                this.DEV_InternalOnDisposeDevView();
            }

            if (this.IsInitDebugInfo == true)
            {
                this.DebugInfoStringBuilder.Clear();

                var msec = this.DeltaTime * 1000.0f;
                var fps = 1.0f / this.DeltaTime;

                if (this.TotalMemoryRecorder.Valid == true)
                {
                    this.DebugInfoStringBuilder.AppendLine($"  FPS : {fps:0.} / msec : {msec:0.0} / Memory : {this.TotalMemoryRecorder.LastValue / 1048576f:0.00}MB");
                }
                else
                {
                    this.DebugInfoStringBuilder.AppendLine($"  FPS : {fps:0.} / msec : {msec:0.0}");
                }

                //if(this.TextureMemoryRecorder.Valid == true && this.TextureCountRecorder.Valid == true)
                //{
                //    this.DebugInfoStringBuilder.AppendLine($"  Texture Memory: {this.TextureMemoryRecorder.LastValue / 1048576f:0.00} MB / Texture Count : {this.TextureCountRecorder.LastValue}");
                //}

                if (this.GCUsedMemoryRecorder.Valid == true && this.GCReservedMemoryRecorder.Valid == true)
                {
                    this.DebugInfoStringBuilder.AppendLine($"  GC Used Memory : {this.GCUsedMemoryRecorder.LastValue / 1048576f:0.00} MB / GC Reserved Memory : {this.GCReservedMemoryRecorder.LastValue / 1048576f:0.00} MB");
                }

                if (this.RenderProfileRecorder.Valid == true && this.CpuProfileRecorder.Valid == true)
                {
                    this.DebugInfoStringBuilder.AppendLine($"  DrawCall : {this.RenderProfileRecorder.LastValue} / CPU : {this.CpuProfileRecorder.LastValue * (1e-6f):F1} ms");
                }

                if (this.RendersetPassCallsRecorder.Valid == true && this.RenderTextureBatchesRecorder.Valid == true)
                {
                    this.DebugInfoStringBuilder.AppendLine($"  SetPass Calls : {this.RendersetPassCallsRecorder.LastValue} / Batches Count : {this.RenderTextureBatchesRecorder.LastValue}");
                }
            }
        }

        private void DEV_InternalOnLoadDevView()
        {
            if (this.IsInitDebugInfo == false)
            {
                this.IsInitDebugInfo = true;
                this.DeltaTime = 0.0f;

                this.CpuProfileRecorder         = Unity.Profiling.ProfilerRecorder.
                    StartNew(Unity.Profiling.ProfilerCategory.Internal, "Main Thread");
                this.TotalMemoryRecorder        = Unity.Profiling.ProfilerRecorder.
                    StartNew(Unity.Profiling.ProfilerCategory.Memory, "Total Used Memory");
                this.RenderProfileRecorder      = Unity.Profiling.ProfilerRecorder.
                    StartNew(Unity.Profiling.ProfilerCategory.Render, "Draw Calls Count");
                this.GCUsedMemoryRecorder       = Unity.Profiling.ProfilerRecorder.
                    StartNew(Unity.Profiling.ProfilerCategory.Memory, "GC Used Memory");
                this.GCReservedMemoryRecorder   = Unity.Profiling.ProfilerRecorder.
                    StartNew(Unity.Profiling.ProfilerCategory.Render, "GC Reserved Memory");
                this.RenderTextureBatchesRecorder   = Unity.Profiling.ProfilerRecorder.
                    StartNew(Unity.Profiling.ProfilerCategory.Render, "Batches Count");
                this.RendersetPassCallsRecorder     = Unity.Profiling.ProfilerRecorder.
                    StartNew(Unity.Profiling.ProfilerCategory.Render, "SetPass Calls Count");

                //this.TextureCountRecorder = Unity.Profiling.ProfilerRecorder.
                //    StartNew(Unity.Profiling.ProfilerCategory.Memory, "Texture Count");

                //this.TextureMemoryRecorder = Unity.Profiling.ProfilerRecorder.
                //    StartNew(Unity.Profiling.ProfilerCategory.Memory, "Texture Memory");
            }
        }
        private void DEV_InternalOnDisposeDevView()
        {
            if (this.IsInitDebugInfo == true)
            {
                this.IsInitDebugInfo = false;
                this.DebugInfoStringBuilder.Clear();
                this.CpuProfileRecorder.Dispose();
                this.TotalMemoryRecorder.Dispose();
                this.RenderProfileRecorder.Dispose();
                this.GCUsedMemoryRecorder.Dispose();
                this.GCReservedMemoryRecorder.Dispose();
                this.RenderTextureBatchesRecorder.Dispose();
                this.RendersetPassCallsRecorder.Dispose();
                //this.TextureCountRecorder.Dispose();
                //this.TextureMemoryRecorder.Dispose();
            }
        }

        void OnGUI()
        {
            if(this.IsRenderDebugInfo == true)
            {
                if (this.IsInitDebugInfo == true)
                {
                    if (this.DrawDebugGUIStyle == null)
                    {
                        var w = Screen.width;
                        var h = Screen.height;
                        this.DrawDebugGUIStyle = new GUIStyle(GUI.skin.box);
                        this.DrawDebugGUIStyle.alignment = TextAnchor.UpperLeft;
                        this.DrawDebugGUIStyle.fontSize = UnityEngine.Mathf.RoundToInt(w * 0.01f);
                        this.DrawDebugGUIStyle.normal.textColor = Color.white;
                    }
                    if (this.DebugInfoStringBuilder.Length > 0)
                    {
                        var w = Screen.width;
                        var h = (UnityEngine.Mathf.RoundToInt(w * 0.01f)) * 5.5f;
                        var y = this.LastScreenSafeArea.y + 10;

                        //y += AdvertisementLogic.Instance.IsShowBanner == true ? 240 : 0;

                        GUI.Box(new Rect(30, y, w / 2.4f, h), this.DebugInfoStringBuilder.ToString(), this.DrawDebugGUIStyle);
                    }
                }
            }
        }
#endif
    }
}
