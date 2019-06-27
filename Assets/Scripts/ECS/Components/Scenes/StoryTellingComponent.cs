using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BeyondPixels.ECS.Components.Game;
using UnityEngine;

namespace BeyondPixels.ECS.Components.Scenes
{
    public class StoryTellingComponent : MonoBehaviour
    {
        public Queue<string> Sentences = new Queue<string>();

        [TextArea(1, 10)]
        [SerializeField]
        private string[] _sentences = new string[0];

        [TextArea(1, 10)]
        [SerializeField]
        private string[] _tips = new string[0];

        public void Start()
        {
            this.Sentences.Clear();
            for (var i = 0; i < this._sentences.Length; i++)
            {
                var text = this._sentences[i];
                this.Sentences.Enqueue(text);
            }
            for (var i = 0; i < this._tips.Length; i++)
            {
                var text = this._tips[i];
                var matches = Regex.Matches(text, @"{([^{}]+)}");
                for (int mI = 0; mI < matches.Count; mI++)
                {
                    var keyBind = matches[mI].Groups[1].Value;
                    if (Enum.TryParse<KeyBindName>(keyBind, out var bindName))
                    {
                        var value = SettingsManager.Instance.GetKeyBindValue(bindName);
                        text = text.Replace(matches[mI].Value, value.ToString().Replace("Alpha", ""));
                    }
                }
                this.Sentences.Enqueue(text);
            }
        }
    }
}
