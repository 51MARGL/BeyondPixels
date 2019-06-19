﻿using System;
using System.Collections.Generic;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Items;

namespace BeyondPixels.ECS.Systems.SaveGame
{
    public interface ISaveData { }

    [Serializable]
    public class SaveData : ISaveData
    {
        public LevelComponent LevelComponent;
        public XPComponent XPComponent;
        public HealthComponent HealthComponent;
        public HealthStatComponent HealthStatComponent;
        public AttackStatComponent AttackStatComponent;
        public DefenceStatComponent DefenceStatComponent;
        public MagicStatComponent MagicStatComponent;

        public List<ItemData> ItemDataList;

    }
    [Serializable]
    public class ItemData
    {
        public bool IsEquiped;
        public ItemComponent ItemComponent;
        public AttackStatModifierComponent AttackModifier;
        public DefenceStatModifierComponent DefenceModifier;
        public HealthStatModifierComponent HealthModifier;
        public MagickStatModifierComponent MagicModifier;
    }

}
