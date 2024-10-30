using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace SCC
{
    //!<===================================================================================
    //IAppContext
    //!<===================================================================================
    public interface IAppContext
    {
        bool IsAvailable                      { get; }

        NetClientLogic NetLogic               { get; }
        SCC.Resources Resources               { get; }
    }

    //!<===================================================================================

    public class AppContext : SCC.SingletonBehaviour<AppContext>, IAppContext
    {
        //!<===============================================================================

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEV_BUILD_VERSION")]
        public static void RuntimeInitializedFromDevBuild()
        {
            Debug.Log(" ========== AppContext.InitializedFromDevBuild ========== ");

            AppContext.Instance.CheckInit();
        }
        //[UnityEditor.InitializeOnEnterPlayMode]
        //private static void OnInitializeOnEnterPlayMode()
        //{
        //    AppContext.Instance.CheckInit();
        //}


        //!<===============================================================================
        //IAppContext
        //!<===============================================================================
        bool IAppContext.IsAvailable        => AppContext.IsAvailable;
        NetClientLogic IAppContext.NetLogic => AppContext.NetLogic;
        SCC.Resources IAppContext.Resources                 => AppContext.Resources;

        //!<===============================================================================

        public enum ServicesState
        {
            Uninitialized = 0,
            Initializing,
            Running,
            InitializedError,
            Destroyed,
        }

        //!<===============================================================================
        //WaitSafeAsync
        //!<===============================================================================
        public static async UniTask WaitSafeAsync()
        {
            if (AppContext._IsAvailable == false)
            {
                return;
            }

            var app = AppContext.Instance;
            if (app.State != ServicesState.Running)
            {
                var inst = false;
                AppContext.GetSafe(i => inst = true);

                await UniTask.WaitUntil(() => inst);
            }
        }
        //!<===============================================================================
        //GetSafeAsync
        //!<===============================================================================
        public static async UniTask<IAppContext> GetSafeAsync()
        {
            if (AppContext._IsAvailable == false)
            {
                return null;
            }
            if (AppContext.Instance.State == ServicesState.Running)
            {
                return AppContext.Instance;
            }
            else
            {
                var inst = false;
                AppContext.GetSafe(i => inst = true);

                await UniTask.WaitUntil(() => inst);

                return AppContext.Instance;
            }
        }
        //!<===============================================================================
        //********************************** Safe Instance ********************************
        //!<===============================================================================
        public static void GetSafe(System.Action<IAppContext> callback)
        {
            if (AppContext._IsAvailable == false)
            {
                callback?.Invoke(null);
                return;
            }

            var instance = AppContext.Instance;

            if (instance.State == ServicesState.Running)
            {
                callback?.Invoke(instance);
                return;
            }
            

            if (instance.State == ServicesState.Uninitialized)
            {
                if (instance.SafeCallbacks.Contains(callback) == false)
                {
                    instance.SafeCallbacks.Add(callback);
                }
                instance.CheckInit();
            }
            else
            {
                if (AppContext.Instance.State == ServicesState.Initializing)
                {
                    if (instance.SafeCallbacks.Contains(callback) == false)
                    {
                        instance.SafeCallbacks.Add(callback);
                    }
                }
                else
                {
                    Debug.LogError("Unknown Error, initialize faield?");
                }
            }
        }
        //!<===============================================================================

        protected static bool _IsAvailable = true;

        //!<===============================================================================

        public static bool IsAvailable => 
            AppContext._IsAvailable         == true && 
            AppContext.HasInstance          == true && 
            AppContext.Instance.IsRunning   == true;

        public static bool IsAbusing    { get; private set; } = false;

        //!<===================================================================================
        //********************************** UnSafe Instance **********************************
        //!<===================================================================================

        public static NetClientLogic            NetLogic        => AppContext.IsAvailable ? AppContext.Instance.NetClient : null;
        public static SCC.Resources             Resources       => AppContext.IsAvailable ? SCC.Resources.Instance : null;

        //!<===============================================================================

        //!<===============================================================================

        public bool IsRunning => this.State == ServicesState.Running;

        public ServicesState State  { get; protected set; } = ServicesState.Uninitialized;

        //!<===============================================================================

        protected List<System.Action<AppContext>> SafeCallbacks = new List<System.Action<AppContext>>();
        protected NetClientLogic NetClient = null;

        //!<===============================================================================

        protected readonly List<AppContextBehaviour> Behaviours = new();

        //!<===============================================================================
        //InternalCreateInstance
        protected T InternalCreateInstance<T>() where T : AppContextBehaviour
        {
            if (SCC.Application.IsPlaying == false)
            {
               Debug.LogError(typeof(T).ToString() + ".Instance where not playing!");
                return null;
            }

            var src_name = typeof(T).ToString();
            if (this.Behaviours.SingleOrDefault(i => i.name == src_name) != null)
            {
                Debug.LogError(typeof(T).ToString() + ".Instance Duplicate");
                return null;
            }

            var g = new GameObject
            {
                name = src_name
            };
            g.transform.parent  = AppContext.Instance.transform;
            T instance          = g.AddComponent<T>();
            g.gameObject.SetActive(true);

            instance.OnCreate();

            this.Behaviours.Add(instance);
            return instance;
        }

        //!<===============================================================================
        public override void CheckInit()
        {
            this.Init();
        }

        protected override void Init()
        {
            if (this.State == ServicesState.Uninitialized)
            {
                this.InternalInitialize();
            }
        }

        protected bool InternalInitialize()
        {
            if (SCC.Application.IsAvailable == false)
            {
                Debug.LogError("SAR.Application.IsAvailable == false");
                this.State = ServicesState.InitializedError;

                return false;
            }

            if (this.State == ServicesState.Uninitialized)
            {
                this.State = ServicesState.Initializing;

                var application     = SCC.Application.Instance;

                application.OnUpdateApplicationPause    -= this.InternalOnUpdateApplicationPause;
                application.OnUpdateApplicationQuit     -= this.InternalOnUpdateApplicationQuit;

                application.OnUpdateApplicationPause   += this.InternalOnUpdateApplicationPause;
                application.OnUpdateApplicationQuit    += this.InternalOnUpdateApplicationQuit;

                this.NetClient = this.InternalCreateInstance<NetClientLogic>();

                this.State = ServicesState.Running;

                AppContext._IsAvailable = true;

                this.Behaviours.ForEach(i => i.OnCreate());
                this.Behaviours.ForEach(i => i.CheckInit());
                this.Behaviours.ForEach(i => i.OnStartup(this));
            }

            return true;
        }

        protected void InternalOnUpdateApplicationPause(bool pause)
        {

        }
        protected void InternalOnUpdateApplicationQuit()
        {
            if(AppContext.IsAvailable == true)
            {
                this.Behaviours.ForEach(i => i.OnBeforeAplicationQuit());
                this.Behaviours.Clear();
                AppContext._IsAvailable = false;
            }
        }
    }

}