using System;
using UnityEngine;

namespace BeyondPixels.ECS.Components.Scenes
{
    public class SceneFadeManager : MonoBehaviour
    {
        public static SceneFadeManager Instance { get; private set; }
        public Animator Animator;
        public event Action OnFadeOutEvent; 

        public void Awake()
        {
            SceneFadeManager.Instance = this;
        }

        public void OnFadeOut()
        {
            this.OnFadeOutEvent?.Invoke();
        }
    }
}
