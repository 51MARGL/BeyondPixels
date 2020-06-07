using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.Quest;
using BeyondPixels.ECS.Components.SaveGame;

using System;
using System.Collections.Generic;

namespace BeyondPixels.ECS.Systems.SaveGame
{
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
        public List<QuestData> QuestDataList;
    }

    [Serializable]
    public class ItemData
    {
        public bool IsEquiped;
        public ItemComponent ItemComponent;
        public AttackStatModifierComponent AttackModifier;
        public DefenceStatModifierComponent DefenceModifier;
        public HealthStatModifierComponent HealthModifier;
        public MagicStatModifierComponent MagicModifier;
    }

    [Serializable]
    public class QuestData
    {
        public string QuestText;
        public QuestComponent QuestComponent;
        public PickUpQuestComponent PickUpQuestComponent;
        public XPRewardComponent XPRewardComponent;
        public LevelComponent LevelComponent;
        public bool IsDone;
        public bool IsDefeatQuest;
        public bool IsInvestigateQuest;
        public bool IsLevelUpQuest;
        public bool IsLootQuest;
        public bool IsPickUpQuest;
        public bool IsReleaseQuest;
        public bool IsSpendQuest;
    }
}
