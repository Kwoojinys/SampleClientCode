using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SCC.UI
{
    //!<===============================================================================
    //UpdateJobScreenViewUI
    //!<===============================================================================
    public class UpdateJobScreenViewUI : MonoBehaviour
    {
        //!<===========================================================================

        [SerializeField] protected CanvasGroup MainCanvasView;
        [SerializeField] protected Image ProgressBarImage;

        //!<============================================================================

        private Coroutine _CheckCoroutine = null;
        private int _UpdateJobCount = 0;

        //!<============================================================================

        public bool IsActiveLogic { get; private set; } = false;

        //!<============================================================================

        private void Awake()
        {
            this.MainCanvasView.alpha = 0;
            this._UpdateJobCount      = 0;
        }
        private void OnDisable()
        {
            this._CheckCoroutine = null;

            this.CancelInvoke();
            this.StopAllCoroutines();
        }
        private void InternalOnFire(float delay, float autoHide = 0.2f)
        {
            if (this.IsActiveLogic == false)
            {
                if (this.gameObject.activeInHierarchy == false)
                {
                    this.gameObject.SetActive(true);
                }

                this.gameObject.transform.SetAsLastSibling();
                this.IsActiveLogic = true;
            }

            if (autoHide > 0.0f)
            {
                if (this._CheckCoroutine != null)
                {
                    this.StopCoroutine(this._CheckCoroutine);
                    this._CheckCoroutine = null;
                }

                this._CheckCoroutine = this.StartCoroutine(this.CheckLimitedTime(autoHide));
            }
        }
        private IEnumerator CheckLimitedTime(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (this.IsActiveLogic == true)
            {
                this.FireEndJob();
            }
        }
        private void InternalDoCleanup()
        {
            if (this.IsActiveLogic == true)
            {
                this.IsActiveLogic = false;

                if (this._CheckCoroutine != null)
                {
                    this.StopCoroutine(this._CheckCoroutine);
                    this._CheckCoroutine = null;
                }

                this.gameObject.SetActive(false);
            }
        }

        public void FireBeginJob(float delay = 0.2f, float autoHide = 0.0f)
        {
            this._UpdateJobCount++;

            if (this._UpdateJobCount > 0)
            {
                this.InternalOnFire(delay, autoHide);
            }
        }

        public void FireEndJob()
        {
            if (this.IsActiveLogic == true)
            {
                this._UpdateJobCount--;

                if (this._UpdateJobCount <= 0)
                {
                    this._UpdateJobCount = 0;

                    this.InternalDoCleanup();
                }
            }
        }

        //!<============================================================================
        //LoadScreenViewUI
        //!<============================================================================
        public static UpdateJobScreenViewUI LoadScreenViewUI(Transform parent)
        {
            var child = parent.Find("JobScreenViewUI");
            if (child == null)
            {
                var prefabPath = "00.Prefabs/01.Common/JobScreenViewUI";
                var view = SCC.Foundation.InternalResources.Instantiate<UpdateJobScreenViewUI>(prefabPath, parent);
                view.name = "JobScreenViewUI";
                return view;
            }
            else
            {
                return child.GetComponent<UpdateJobScreenViewUI>();
            }
        }
    }
}
