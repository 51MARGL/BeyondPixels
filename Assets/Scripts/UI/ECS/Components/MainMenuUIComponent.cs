using BeyondPixels.ECS.Components.SaveGame;
using BeyondPixels.UI.Buttons;
using BeyondPixels.UI.Menus;
using UnityEngine;

namespace BeyondPixels.UI.ECS.Components
{
    public class MainMenuUIComponent : MenuWithNavigationButtonsUI
    {
        public bool IgnoreEsc;
        public bool InGameMenu;
        public SubmitButton ResumeButton;
        public SubmitButton LoadLastButton;

        public override void Show()
        {
            base.Show();
            Time.timeScale = 0f;
            this.ResumeButton.gameObject.SetActive(this.InGameMenu);
            this.LoadLastButton.gameObject.SetActive(SaveGameManager.SaveExists);
        }

        public override void Hide()
        {
            base.Hide();
            Time.timeScale = 1f;
        }
    }
}
