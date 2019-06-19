using TMPro;
using Unity.Entities;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeyondPixels.UI.Buttons
{
    public abstract class ItemButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Image ItemIcon;
        public Entity ItemEntity;
        public TextMeshProUGUI Amount;

        public abstract void OnPointerClick(PointerEventData eventData);

        public abstract void OnPointerEnter(PointerEventData eventData);

        public void OnPointerExit(PointerEventData eventData)
        {
            UIManager.Instance.HideTooltip();
        }
    }
}
