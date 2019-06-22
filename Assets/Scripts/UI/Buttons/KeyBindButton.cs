using BeyondPixels.ECS.Components.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeyondPixels.UI.Buttons
{
    public class KeyBindButton : SubmitButton
    {
        public KeyBindName BindName;
        public TextMeshProUGUI KeyText;

        public void SetPending()
        {
            this.GetComponent<Image>().color = Color.green;
        }

        public void SetDefault()
        {
            this.GetComponent<Image>().color = Color.white;
        }

        public void UpdateText(string text)
        {
            this.KeyText.text = text.Replace("Alpha", "");
        }
    }
}
