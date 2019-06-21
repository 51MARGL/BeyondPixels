using System.Linq;
using BeyondPixels.ECS.Components.Game;
using BeyondPixels.UI.Buttons;
using BeyondPixels.UI.Menus;
using UnityEngine;
using UnityEngine.UI;

namespace BeyondPixels.UI.ECS.Components
{
    public class OptionsMenuUIComponent : MenuWithNavigationButtonsUI
    {
        public Toggle FullScreenToggle;
        public Dropdown ResolutionDropdown;
        public KeyBindButton[] KeyBindButtons;

        public KeyBindButton PendingBindButton { get; set; }

        private bool SkipFrame;
        private KeyCode[] Keys;

        public void Start()
        {
            this.Keys = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
            this.KeyBindButtons = GameObject.FindGameObjectsWithTag("KeyBindButton")
                            .Select(k => k.GetComponent<KeyBindButton>()).ToArray();

            for (var i = 0; i < this.KeyBindButtons.Length; i++)
            {
                var index = i;
                var keyBindButton = this.KeyBindButtons[index];
                keyBindButton.UpdateText(
                    SettingsManager.Instance.GetKeyBindValue(keyBindButton.BindName).ToString().Replace("Alpha", ""));
                keyBindButton.OnSubmitEvent += () =>
                {
                    if (this.PendingBindButton == null)
                        this.SetKeyBind(keyBindButton);
                };
            }

            this.ResolutionDropdown.options = SettingsManager.Instance.Resolutions
                .Select(r => new Dropdown.OptionData(r.width + "x" + r.height + " " + r.refreshRate + "Hz"))
                .ToList();

            this.FullScreenToggle.isOn = SettingsManager.Instance.Fullscreen;
            this.ResolutionDropdown.value = SettingsManager.Instance.CurrentResolutionIndex;
        }

        public override void Show()
        {
            base.Show();

            UIManager.Instance.MainMenu.Hide();
            Time.timeScale = 0f;

            this.RefreshControls();
        }

        public override void Hide()
        {
            base.Hide();

            SettingsManager.Instance.SaveSettings();
            UIManager.Instance.MainMenu.Show();
        }

        public void SetResolution(int index)
        {
            SettingsManager.Instance.SetResolution(index);
            this.RefreshControls();
        }

        public void SetFullScreen(bool value)
        {
            SettingsManager.Instance.SetFullScreen(value);
            this.RefreshControls();
        }

        public void SetKeyBind(KeyBindButton keyBind)
        {
            this.PendingBindButton = keyBind;
            this.PendingBindButton.SetPending();
            this.SkipFrame = true;
        }

        public void RefreshControls()
        {
            this.FullScreenToggle.isOn = SettingsManager.Instance.Fullscreen;

            for (var i = 0; i < this.KeyBindButtons.Length; i++)
            {
                var keyBindButton = this.KeyBindButtons[i];
                keyBindButton.UpdateText(
                    SettingsManager.Instance.GetKeyBindValue(keyBindButton.BindName).ToString());
            }
        }

        public void Update()
        {
            if (this.PendingBindButton != null)
            {
                if (this.SkipFrame)
                {
                    this.SkipFrame = false;
                    return;
                }

                for (var i = 0; i < this.Keys.Length; i++)
                {
                    var vKey = this.Keys[i];

                    if (Input.GetKeyDown(vKey))
                    {
                        if (vKey == KeyCode.Escape)
                        {
                            this.PendingBindButton.SetDefault();
                            this.PendingBindButton = null;
                            UIManager.Instance.MainMenu.IgnoreEsc = true;
                            return;
                        }

                        SettingsManager.Instance.SetKeyBind(this.PendingBindButton.BindName, vKey);
                        this.PendingBindButton.UpdateText(vKey.ToString().Replace("Alpha", ""));
                        this.PendingBindButton.SetDefault();
                        this.PendingBindButton = null;

                        this.RefreshControls();

                        return;
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                UIManager.Instance.MainMenu.IgnoreEsc = true;
                this.Hide();
            }
        }
    }
}
