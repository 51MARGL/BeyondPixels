using BeyondPixels.UI.ECS.Components;
using UnityEngine;

namespace BeyondPixels.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        public GameUIComponent GameUIComponent;
        public PlayerInfoMenuUIComponent PlayerInfoMenuUIComponent;

        public void Start()
        {
            UIManager.Instance = this;
        }
    }
}
