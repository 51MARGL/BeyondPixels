using UnityEngine;
using UnityEngine.EventSystems;

namespace BeyondPixels.UI.Buttons
{
    public abstract class SubmitButton : MonoBehaviour, ISubmitHandler, IPointerClickHandler
    {
        protected abstract void Submit();

        public void OnSubmit(BaseEventData eventData)
        {
            this.Submit();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                this.Submit();
        }
    }
}
