using BeyondPixels.UI.Buttons;
using BeyondPixels.UI.Menus;

using System;

using TMPro;

using UnityEngine;

namespace BeyondPixels.UI.ECS.Components
{
    public class YesNoDialogUIComponent : MenuWithNavigationButtonsUI, IDisposable
    {
        public TextMeshProUGUI Text;
        public SubmitButton YesButton;
        public SubmitButton NoButton;
        public event Action OnCloseEvent;

        public void Start()
        {
            this.YesButton.OnSubmitEvent += this.Hide;
            this.NoButton.OnSubmitEvent += this.Hide;
            this.YesButton.OnCancelEvent += this.Hide;
            this.NoButton.OnCancelEvent += this.Hide;
        }

        public override void Hide()
        {
            base.Hide();

            this.OnCloseEvent?.Invoke();

            this.Dispose();
        }

        public void Dispose()
        {
            this.YesButton.OnSubmitEvent -= this.Hide;
            this.NoButton.OnSubmitEvent -= this.Hide;
            this.YesButton.OnCancelEvent -= this.Hide;
            this.NoButton.OnCancelEvent -= this.Hide;

            GameObject.Destroy(this.gameObject);
        }
    }
}
