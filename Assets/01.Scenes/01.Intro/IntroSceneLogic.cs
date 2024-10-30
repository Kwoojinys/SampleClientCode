using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SAR
{
    public class IntroSceneLogic : MonoBehaviour
    {
        //!<=======================================================================================

        [SerializeField] protected TextMeshProUGUI StatusText;
        [SerializeField] protected Image ProgressBar;

        //!<=======================================================================================

        protected bool IsLoadedResources = false;

        //!<=======================================================================================


        void Start()
        {
            this.StatusText.text = "";
            this.ProgressBar.fillAmount = 0;
            this.InternalLoadedAddressble();
        }

        protected void InternalLoadedAddressble()
        {
            SCC.AddressableAssetsService.GetSafeService(i =>
            {
                this.IsLoadedResources = true;

                this.StartCoroutine(this.InternalLoadStartScene());
            });
        }

        protected IEnumerator InternalLoadStartScene()
        {
            this.ProgressBar.enabled = true;
            this.StatusText.enabled = true;
            var startscene  = StartSceneLogic.SCENE_NAME;
            var async       = SceneManager.LoadSceneAsync(startscene);
            async.allowSceneActivation = false;
            var isDone = false;
            while (async.isDone == false && isDone == false)
            {
                this.ProgressBar.fillAmount = async.progress / 0.9f;
                if (async.progress >= 0.9f)
                {
                    this.StatusText.text = $"LOADING...";

                    yield return new WaitUntil(() =>

                    this.IsLoadedResources == true);

                    isDone = true;
                    async.allowSceneActivation = true;
                }
                else
                {
                    this.StatusText.text = $"LOADING...( {this.ProgressBar.fillAmount * 100.0f}% )";
                }
            }
        }
    }

}
