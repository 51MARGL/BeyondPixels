using BeyondPixels.ECS.Components.Spells;
using BeyondPixels.UI.ECS.Components;
using UnityEngine;

namespace BeyondPixels.UI
{
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<UIManager>();

                return _instance;
            }
        }
        public UIComponent UIComponent;
        public void Initialize()
        {
            this.UIComponent = GetComponent<UIComponent>();
        }
    }
}
