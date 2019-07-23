using System;
using BeyondPixels.UI.Buttons;
using BeyondPixels.UI.Menus;
using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace BeyondPixels.UI.ECS.Components
{
    public class PlayerInfoMenuUIComponent : MenuUI
    {
        public LevelGroupWrapper LevelGroup;
        public StatsGroupWrapper StatsGroup;
        public EquipedGearGroupWrapper GearGroup;
        public InventoryGroupWrapper InventoryGroup;

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
        public class EquipedGearGroupWrapper
        {
            public EquipedGearButton[] GearSlots;
        }

        [Serializable]
        public class InventoryGroupWrapper
        {
            public GameObject Grid;
            public GameObject ItemButtonPrefab;
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
