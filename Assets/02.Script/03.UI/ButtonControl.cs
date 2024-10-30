using UnityEngine;
using UnityEngine.UI;


namespace SCC.UI.Foundation
{
    //!<===============================================================================
    //ButtonControl
    //!<===============================================================================
    [RequireComponent(typeof(Button))]
    public class ButtonControl : MonoBehaviour
    {
        //!<=========================================================================

        [SerializeField] protected bool SingleAction = true;

        //!<=========================================================================

        public System.Action Action;

        //!<=========================================================================

        protected Button _ButtonPtr;

        //!<=========================================================================

        public Button Button
        {
            get
            {
                return this._ButtonPtr ?? (this._ButtonPtr = this.GetComponent<Button>());
            }
        }
        public bool IsInteractable
        {
            get
            {
                return this.Button.interactable;
            }
            set
            {
                this.Button.interactable = value;
            }
        }

        //!<=========================================================================

        public void SetInteractable(bool b)
        {
            this.IsInteractable = b;
        }
        protected virtual void Start()
        {
            if(this.SingleAction == true)
            {
                this.Button.onClick.RemoveAllListeners();
            }

            this.Button.onClick.AddListener(this.OnClick);
        }

        protected virtual void OnClick()
        {
            if (this.Action != null)
            {
                this.Action.Invoke();
            }
        }

        public virtual void SetHighlighted(bool v)
        {
            var color = this.Button.targetGraphic.color;
            color.a = v ? 1 : 0.5f;
            this.Button.targetGraphic.color = color;
        }

        public virtual void SetBlendedHighlight(float f)
        {
            var color = this.Button.targetGraphic.color;
            color.a = Mathf.Lerp(1.0f, 0.5f, f);
            this.Button.targetGraphic.color = color;
        }
    }
}