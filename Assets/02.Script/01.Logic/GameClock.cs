using UnityEngine;

namespace SCC
{
    [AddComponentMenu("")]
    public sealed class GameClock : SCC.SingletonBehaviour<GameClock>
    {
        //!<============================================================================

        private float _TimeScale = 1.0f;
        private float _AnimTimeScale = 1.0f;

        //!<============================================================================

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnFireBeforeSceneLoad()
        {
            GameClock.Instance.CheckInit();
        }

        protected override void Init()
        {
            this._TimeScale = 1.0f;
        }
        public static float FixedDeltaTime
        {
            get { return UnityEngine.Time.fixedDeltaTime * GameClock.TimeScale; }
        }
        public static float TimeScale
        {
            get { return GameClock.Instance._TimeScale; }
            set { GameClock.Instance._TimeScale = value; }
        }
        public static float AnimationTimeScale
        {
            get { return GameClock.Instance._AnimTimeScale; }
            set { GameClock.Instance._AnimTimeScale = value; }
        }
        public static float DeltaTime
        {
            get { return UnityEngine.Time.deltaTime * GameClock.TimeScale; }
        }
        public static float UnscaledDeltaTime
        {
            get { return UnityEngine.Time.unscaledDeltaTime; }
        }
        public static float Time
        {
            get { return UnityEngine.Time.time; }
        }
        public static float AnimationDeltaTime
        {
            get { return UnityEngine.Time.deltaTime * GameClock.AnimationTimeScale; }
        }
    }
}