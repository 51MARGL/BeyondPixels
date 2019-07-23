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

        public virtual void OnSubmit(BaseEventData eventData)
        {
            this.OnSubmitEvent();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                this.OnSubmitEvent?.Invoke();
        }

        public virtual void OnCancel(BaseEventData eventData)
        {
            this.OnCancelEvent?.Invoke();
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            this.GetComponent<Selectable>().OnPointerExit(null);
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (!EventSystem.current.alreadySelecting)
                EventSystem.current.SetSelectedGameObject(this.gameObject);
        }
    }
}
