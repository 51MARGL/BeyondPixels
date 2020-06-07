using BeyondPixels.ECS.Components.Scenes;
using BeyondPixels.UI.Buttons;
using BeyondPixels.UI.Menus;

using System.Collections;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace BeyondPixels.UI.ECS.Components
{
    public class StoryMenuUIComponent : MenuWithNavigationButtonsUI
    {
        public TextMeshProUGUI Text;
        public SubmitButton ContinueButton;
        public Coroutine PrintCoroutine;
        public string CurrentSentence;
        public StoryTellingComponent StoryTellingComponent;
        public Image Background;

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

        public void TellStory(StoryTellingComponent storyTellingComponent)
        {
            this.StoryTellingComponent = storyTellingComponent;

            this.PrintCoroutine = this.StartCoroutine(this.PrintSentence());
            this.ContinueButton.OnSubmitEvent += this.OnContinuePressed;
        }

        private void OnContinuePressed()
        {
            if (this.PrintCoroutine != null)
            {
                this.StopCoroutine(this.PrintCoroutine);
                this.PrintCoroutine = null;

                this.Text.text = this.CurrentSentence;
            }
            else
            {
                this.PrintCoroutine = this.StartCoroutine(this.PrintSentence());
            }
        }

        private IEnumerator PrintSentence()
        {
            if (this.StoryTellingComponent.Sentences.Count > 0)
            {
                this.CurrentSentence = this.StoryTellingComponent.Sentences.Dequeue();

                if (this.CurrentSentence.Contains("Tip:"))
                {
                    this.Background.color = new Color(0.3f, 0.3f, 0.3f);
                }
                else
                {
                    this.Background.color = Color.black;
                }

                var chars = this.CurrentSentence.ToCharArray();

                this.Text.text = "";
                for (var i = 0; i < chars.Length; i++)
                {
                    this.Text.text += chars[i];
                    yield return null;
                }
            }
            else
            {
                this.ContinueButton.OnSubmitEvent -= this.OnContinuePressed;
                this.CurrentSentence = null;
                this.Hide();
            }

            this.PrintCoroutine = null;
        }
    }
}
