using System.Collections.Generic;
using UnityEngine;

namespace BeyondPixels.ECS.Components.Scenes
{
    public class StoryTellingComponent : MonoBehaviour
    {
        public Queue<string> Sentences = new Queue<string>();

        [TextArea(1, 10)]
        [SerializeField]
        private string[] _sentences;

        public void Start()
        {
            this.Sentences.Clear();
            foreach (var item in this._sentences)
            {
                this.Sentences.Enqueue(item);
            }
        }
    }
}
