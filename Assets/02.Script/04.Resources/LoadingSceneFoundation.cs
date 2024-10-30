using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace SCC.Foundation
{
    //!<===============================================================================

    public class LoadingSceneBaseLogic : MonoBehaviour
    {
        //!<===========================================================================

        [SerializeField] protected TextMeshProUGUI  StatusText;
        [SerializeField] protected Image            ProgressBar;
        [SerializeField] protected RectTransform    MainView;

        //!<===========================================================================
    }

    //!<===============================================================================
    public struct IngameLoadingSceneOrder
    {
        public static readonly IngameLoadingSceneOrder INVALID = new() 
        {
            FromScene = null,
            FetchName = null,
            Callback  = null,
        };
        public SCC.SceneFoundation FromScene  { get; set; }
        public string FetchName              { get; set; }
        public System.Action<Scene> Callback { get; set; }
        public bool IsValid => string.IsNullOrEmpty(this.FetchName) == false;
    }
    //!<===============================================================================
    public interface ILoadingSceneFoundation
    {
        void LoadLevel(IngameLoadingSceneOrder order);
    }
    //!<===============================================================================
    //!<===============================================================================
    //LoadingSceneFoundation
    //!<===============================================================================
    public class LoadingSceneFoundation : ILoadingSceneFoundation
    {
       
        //!<============================================================================

        private static ILoadingSceneFoundation _DefaultFoundation = null;
        internal static ILoadingSceneFoundation ActiveFoundation
        {
            get
            {
                if (LoadingSceneFoundation.OverrideFoundation != null)
                {
                    return LoadingSceneFoundation.OverrideFoundation;
                }
                else
                {
                    LoadingSceneFoundation._DefaultFoundation ??= new LoadingSceneFoundation();

                    return LoadingSceneFoundation._DefaultFoundation;
                }
            }
        }

        internal static ILoadingSceneFoundation _OverrideFoundation;

        public static ILoadingSceneFoundation OverrideFoundation
        {
            get => LoadingSceneFoundation._OverrideFoundation;
            set
            {
                if (LoadingSceneFoundation._DefaultFoundation != null)
                {
                    LoadingSceneFoundation._DefaultFoundation = null;
                }
                else
                {
                    LoadingSceneFoundation._OverrideFoundation = value;
                }
            }
        }
        public static string PrevSceneName    { get; protected set; }
        public static string CurrentSceneName { get; protected set; }

        //!<============================================================================

        public static void LoadLevel(SCC.SceneFoundation scene, string fetch, System.Action<Scene> callback)
        {
            if (scene != null)
            {
                LoadingSceneFoundation.PrevSceneName    = LoadingSceneFoundation.CurrentSceneName;
                LoadingSceneFoundation.CurrentSceneName = scene.name;

                //TF.GameClock.TimeScale = 1;
                //TF.GameClock.AnimationTimeScale = 1;

                SCC.Foundation.Application.Instance.OnUnloadScene(LoadingSceneFoundation.PrevSceneName);
                var canvas = scene?.SystemPopupCanvas ?? null;
                if (canvas != null)
                {
                    LoadingSceneFoundation.ActiveFoundation
                        .LoadLevel(new IngameLoadingSceneOrder()
                        {
                            FetchName = fetch,
                            FromScene = scene,
                            Callback = callback
                        });
                }
                else
                {
                    LoadingSceneFoundation.ActiveFoundation
                        .LoadLevel(new IngameLoadingSceneOrder()
                        {
                            FetchName = fetch,
                            FromScene = scene,
                        });
                }
            }
            else
            {
                LoadingSceneFoundation.ActiveFoundation
                       .LoadLevel(new IngameLoadingSceneOrder()
                       {
                           FetchName = fetch,
                           FromScene = null,
                       });
            }
        }

        void ILoadingSceneFoundation.LoadLevel(IngameLoadingSceneOrder order)
        {
            SceneManager.LoadScene(order.FetchName);
        }
        
    }
}
