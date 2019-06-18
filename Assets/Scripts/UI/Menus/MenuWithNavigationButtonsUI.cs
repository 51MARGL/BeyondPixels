using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeyondPixels.UI.Menus
{
    public abstract class MenuWithNavigationButtonsUI : MenuUI
    {
        public Button DefaultSelected;

        public override void Show()
        {
            base.Show();

            EventSystem.current.SetSelectedGameObject(null);
            this.DefaultSelected.Select();
        }
    }
}
