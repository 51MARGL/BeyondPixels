using BeyondPixels.UI.ECS.Components;

using System;

using UnityEngine.EventSystems;

namespace BeyondPixels.UI.Buttons
{
    public class SubmitConfirmButton : SubmitButton
    {
        public string DialogText = "Are You sure?";
        public YesNoDialogUIComponent ConfirmDialog;

        public void Start()
        {
            this.OnSubmitEvent += this.InitConfirmDialog;
        }

        protected virtual void InitConfirmDialog()
        {
            this.ConfirmDialog = UIManager.Instance.CreateYesNoDialog();
            this.ConfirmDialog.Text.text = this.DialogText.Replace("\\n", Environment.NewLine);

            this.ConfirmDialog.OnCloseEvent += () =>
            {
                if (!EventSystem.current.alreadySelecting)
                {
                    EventSystem.current.SetSelectedGameObject(this.gameObject);
                }
            };

            this.ConfirmDialog.Show();
        }
    }
}
