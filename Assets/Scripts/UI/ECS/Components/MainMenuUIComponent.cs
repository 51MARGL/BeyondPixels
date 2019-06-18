using BeyondPixels.UI.Menus;
using UnityEngine;

namespace BeyondPixels.UI.ECS.Components
{
    public class MainMenuUIComponent : MenuWithNavigationButtonsUI
    {
        public override void Show()
        {
            base.Show();
            Time.timeScale = 0f;
        }

        public override void Hide()
        {
            base.Hide();
            Time.timeScale = 1f;
        }
    }
}
