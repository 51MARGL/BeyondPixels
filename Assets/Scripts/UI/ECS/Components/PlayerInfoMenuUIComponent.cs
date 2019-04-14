using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeyondPixels.UI.ECS.Components
{
    public class PlayerInfoMenuUIComponent : MonoBehaviour
    {
        public LevelGroupWrapper LevelGroup;
        public StatsGroupWrapper StatsGroup;

        [Serializable]
        public class LevelGroupWrapper
        {
            public TextMeshProUGUI Level;
            public TextMeshProUGUI SkillPoints;
        }

        [Serializable]
        public class StatsGroupWrapper
        {
            public HealthStatWrapper HealthStat;
            public AttackStatWrapper AttackStat;
            public DefenceStatWrapper DefenceStat;
            public MagicStatWrapper MagicStat;
        }

        [Serializable]
        public class StatWrapper
        {
            public TextMeshProUGUI PointsSpent;
            public Button AddButton;
        }

        [Serializable]
        public class HealthStatWrapper : StatWrapper { }
        [Serializable]
        public class AttackStatWrapper : StatWrapper { }
        [Serializable]
        public class DefenceStatWrapper : StatWrapper { }
        [Serializable]
        public class MagicStatWrapper : StatWrapper { }
    }
}
