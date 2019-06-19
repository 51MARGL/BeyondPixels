using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeyondPixels.UI.Menus
{
    public abstract class MenuWithNavigationButtonsUI : MenuUI, IPointerClickHandler
    {
        public Button DefaultSelected;

        public void OnPointerClick(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(null);
            this.DefaultSelected.Select();
        }

        public override void Show()
        {
            base.Show();

            EventSystem.current.SetSelectedGameObject(null);
            this.DefaultSelected.Select();
        }
    }
}
