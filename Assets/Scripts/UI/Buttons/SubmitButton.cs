using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeyondPixels.UI.Buttons
{
    public class SubmitButton : MonoBehaviour, ISubmitHandler, IPointerClickHandler, ICancelHandler, IPointerEnterHandler, IDeselectHandler
    {
        public event Action OnSubmitEvent;
        public event Action OnCancelEvent;

        public void OnSubmit(BaseEventData eventData)
        {
            this.OnSubmitEvent();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                this.OnSubmitEvent();
        }

        public void OnCancel(BaseEventData eventData)
        {
            this.OnCancelEvent();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            this.GetComponent<Selectable>().OnPointerExit(null);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!EventSystem.current.alreadySelecting)
                EventSystem.current.SetSelectedGameObject(this.gameObject);
        }
    }
}
