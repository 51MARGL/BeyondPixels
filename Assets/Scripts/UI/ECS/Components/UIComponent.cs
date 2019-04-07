using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeyondPixels.UI.ECS.Components
{
    public class UIComponent : MonoBehaviour
    {
        public HealthGroupWrapper HealthGroup;
        public SpellCastBarGroupWrapper SpellCastBarGroup;
        public SpellButtonsGroupWrapper SpellButtonsGroup;

        [Serializable]
        public class HealthGroupWrapper
        {
            public Image HealthImage;
            public TextMeshProUGUI HealthText;
        }

        [Serializable]
        public class SpellCastBarGroupWrapper {
            public CanvasGroup SpellCastCanvasGroup;
            public Image SpellCastBar;
            public TextMeshProUGUI SpellCastTime;
            public Image SpellCastIcon;
            public TextMeshProUGUI SpellCastName;
        }

        [Serializable]
        public class SpellButtonsGroupWrapper
        {
            public SpellActionButton[] ActionButtons;
        }
    }
}
