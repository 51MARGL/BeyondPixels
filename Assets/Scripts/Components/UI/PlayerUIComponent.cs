using System;
using BeyondPixels.UI;
using UnityEngine;
using UnityEngine.UI;

namespace BeyondPixels.Components.UI
{
    public class PlayerUIComponent : MonoBehaviour
    {
        public HealthGroupWrapper HealthGroup;
        public SpellCastBarGroupWrapper SpellCastBarGroup;
        public SpellButtonsGroupWrapper SpellButtonsGroup;

        [Serializable]
        public class HealthGroupWrapper
        {
            public Image HealthImage;
            public Text HealthText;
        }

        [Serializable]
        public class SpellCastBarGroupWrapper {
            public CanvasGroup SpellCastCanvasGroup;
            public Image SpellCastBar;
            public Text SpellCastTime;
            public Image SpellCastIcon;
            public Text SpellCastName;
        }

        [Serializable]
        public class SpellButtonsGroupWrapper
        {
            public SpellActionButton[] ActionButtons;
        }
    }
}
