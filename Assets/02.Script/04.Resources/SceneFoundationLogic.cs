using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using SCC.UI;
using SCC.Foundation;
using SCC.UTIL;

namespace SCC
{
    //!<=================================================================================
    //SceneFoundation
    //!<=================================================================================

    public abstract class SceneFoundation : MonoBehaviour
    {
        //!<==============================================================================

        public abstract string SceneName            { get; }
        public static SceneFoundation Current       { get; private set; }
        public static string PreSceneName           { get; private set; } = "";
        public static bool IsAdressbleScene => false;

        //!<==============================================================================

        public static string FatalError             { get; private set; } = null;
        public static bool IsFatalError => (string.IsNullOrEmpty(SCC.SceneFoundation.FatalError) == false);
        static private readonly float InchToCm = 2.54f;

        //!<============================================================================

        [SerializeField] private Camera _MainCamera;
        [SerializeField] private Camera _MainUICamera;
        [SerializeField] private Canvas _MainCanvas;
        [SerializeField] private Canvas _PopupCanvas;
        [SerializeField] private Canvas _SystemPopupCanvas;
        [SerializeField] private float _DragThresholdCM = 0.5f;

        //!<============================================================================

        public RectTransform MainCanvasContentView          { get; private set; }
        public RectTransform PopupCanvasContentView         { get; private set; }
        public RectTransform SystemPopupCanvasContentView   { get; private set; }

        public Canvas MainCanvas    => this._MainCanvas;
        public Canvas PopupCanvas   => this._PopupCanvas;
        public Canvas SystemPopupCanvas => this._SystemPopupCanvas;

        public Camera MainUICamera  => this._MainUICamera;
        public Camera MainCamera    => this._MainCamera;

        //!<============================================================================

        protected float sceneFadeInTime = 0.5f;

        //!<============================================================================

        private UpdateJobScreenViewUI _UpdateJobScreenViewUI = null;

        //!<============================================================================
        //LoadLevel
        //!<============================================================================
        protected static void LoadLevel(
            SCC.SceneFoundation scene, 
            string fetchscene,
            System.Action<Scene> callback = null)
        {
            SceneFoundation.PreSceneName = string.Empty;
            var current = SceneFoundation.Current;
            if (current != null)
            {
                current.OnFireSceneQuit();
                SceneFoundation.PreSceneName = current.SceneName;
            }
            SceneFoundation.Current = null;
            LoadingSceneFoundation.LoadLevel(scene, fetchscene, callback);
        }
        //!<============================================================================

        public static void OnFireFatalError(string error)
        {
            var current = SCC.SceneFoundation.Current;
            if (current != null && current.SystemPopupCanvas != null)
            {
                Debug.LogError(error);
            }
            else
            {
                SCC.SceneFoundation.FatalError = error;
            }
        }
        //!<============================================================================
        protected virtual void OnFireSceneQuit()
        {

        }
        protected virtual void Awake()
        {
            SceneFoundation.Current = this;

            if (this._MainCanvas != null)
            {
                this.MainCanvasContentView = this._MainCanvas.transform
                    .Find("MainView").GetComponent<RectTransform>();
            }

            if (this._PopupCanvas != null)
            {
                this.PopupCanvasContentView = this._PopupCanvas.transform
                    .Find("MainView")
                    .GetComponent<RectTransform>();
            }
            if (this.SystemPopupCanvas != null)
            {
                this.SystemPopupCanvasContentView = this._SystemPopupCanvas.transform
                    .Find("MainView")
                    .GetComponent<RectTransform>();

                if(this._UpdateJobScreenViewUI == null)
                {
                    this._UpdateJobScreenViewUI =
                        UpdateJobScreenViewUI.LoadScreenViewUI(this.SystemPopupCanvas.transform);
                    this._UpdateJobScreenViewUI.gameObject.SetActive(false);
                }
            }
        }

        protected virtual void Start()
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.pixelDragThreshold
                    = Mathf.Max(5, (int)(this._DragThresholdCM * Screen.dpi / SCC.SceneFoundation.InchToCm));
            }
            else
            {
                Debug.LogError("EventSystem not found!");
            }

            if (SCC.SceneFoundation.IsFatalError == true)
            {
                SCC.SceneFoundation.OnFireFatalError(SCC.SceneFoundation.FatalError);
            }
            if(SCC.Foundation.Application.IsAvailable == true)
            {
                SCC.Foundation.Application.Instance.OnUpdateApplicationPause         += this.OnUpdateApplicationPause;
                SCC.Foundation.Application.Instance.OnUpdateApplicationQuit          += this.OnUpdateApplicationQuit;
                SCC.Foundation.Application.Instance.OnUpdateApplicationReactivate    += this.OnUpdateApplicationReactivate;
            }
        }
        protected virtual void OnDestroy()
        {
            if (object.ReferenceEquals(SceneFoundation.Current, this))
            {
                SceneFoundation.Current = null;
            }

            if (SCC.Foundation.Application.IsAvailable == true)
            {
                SCC.Foundation.Application.Instance.OnUpdateApplicationPause         -= this.OnUpdateApplicationPause;
                SCC.Foundation.Application.Instance.OnUpdateApplicationQuit          -= this.OnUpdateApplicationQuit;
                SCC.Foundation.Application.Instance.OnUpdateApplicationReactivate    -= this.OnUpdateApplicationReactivate;
            }
        }
        protected virtual void OnUpdateApplicationPause(bool pause)
        {

        }
        protected virtual void OnUpdateApplicationQuit()
        {

        }
        protected virtual void OnUpdateApplicationReactivate()
        {

        }

        protected virtual void Update()
        {
#if DEV_BUILD_VERSION
            if (Input.touchCount == 3 || Input.GetMouseButtonUp(2))
            {
                if (DevConsoleSceneFoundation.ActiveFoundation != null && 
                    DevConsoleSceneFoundation.ActiveFoundation.IsActiveSceneLogic() == false)
                {
                    DevConsoleSceneFoundation.ActiveFoundation.LoadLevel();
                }
            }
#endif
        }
    }
}