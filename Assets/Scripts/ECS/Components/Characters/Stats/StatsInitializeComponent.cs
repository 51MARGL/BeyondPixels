﻿using BeyondPixels.ECS.Components.Characters.Level;

using UnityEngine;

namespace BeyondPixels.ECS.Components.Characters.Stats
{
    public class StatsInitializeComponent : MonoBehaviour
    {
        public LevelComponent LevelComponent;
        public HealthStatComponent HealthStatComponent;
        public AttackStatComponent AttackStatComponent;
        public DefenceStatComponent DefenceStatComponent;
        public MagicStatComponent MagicStatComponent;
        public XPRewardComponent XPRewardComponent;
    }

    public enum StatTarget
    {
        HealthStat = 1,
        AttackStat = 2,
        DefenceStat = 3,
        MagicStat = 4
    }
}
