using SCC;
using SCC.UI.Foundation;
using UnityEngine;

namespace SAR
{
    public class StartSceneLogic : SCC.SceneFoundation
    {
        //!<==============================================================================
        public override string SceneName            => StartSceneLogic.SCENE_NAME;
        public readonly static string SCENE_NAME    = "StartScene";

        //!<==============================================================================

        [SerializeField] protected ButtonControl GameStartButton;

        //!<==============================================================================
        protected override void Awake()
        {
            base.Awake();

            this.GameStartButton.Action = this.InternalOnFireGameStartAction;
        }
        protected override void Start()
        {
            base.Start();

            AppContext.Instance.CheckInit();
        }

        protected void InternalOnFireGameStartAction()
        {
            AppContext.GetSafe(i => 
            {
                LoadLevel(this, string.Empty);
            });
        }

        //!<==============================================================================
    }
}