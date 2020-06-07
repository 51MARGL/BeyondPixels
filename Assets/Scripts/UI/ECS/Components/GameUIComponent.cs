using BeyondPixels.UI.Buttons;

using System;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace BeyondPixels.UI.ECS.Components
{
    public class GameUIComponent : MonoBehaviour
    {
        public HealthGroupWrapper HealthGroup;
        public LevelGroupWrapper LevelGroup;
        public SpellCastBarGroupWrapper SpellCastBarGroup;
        public SpellButtonsGroupWrapper SpellButtonsGroup;
        public GameObject QuestDoneMark;

        [Serializable]
        public class HealthGroupWrapper
        {
            public Image HealthImage;
            public TextMeshProUGUI HealthText;
        }

        [Serializable]
        public class LevelGroupWrapper
        {
            public Image XPProgressImage;
            public TextMeshProUGUI LevelText;
        }

        [Serializable]
        public class SpellCastBarGroupWrapper
        {
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
