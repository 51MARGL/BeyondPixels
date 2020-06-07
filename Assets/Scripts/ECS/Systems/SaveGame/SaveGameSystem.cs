using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.Quest;
using BeyondPixels.ECS.Components.SaveGame;

using System.Collections.Generic;

using Unity.Entities;

namespace BeyondPixels.ECS.Systems.SaveGame
{
    public class SaveGameSystem : ComponentSystem
    {
        private EntityQuery _saveGroup;

        protected override void OnCreate()
        {
            this._saveGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
{
                    typeof(SaveGameComponent)
}
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._saveGroup).ForEach((Entity entity) =>
            {
                SaveData playerData = null;
                this.Entities.WithAll<PlayerComponent>().ForEach((Entity playerEntity) =>
                {
                    playerData = new SaveData
                    {
                        LevelComponent = this.EntityManager.GetComponentData<LevelComponent>(playerEntity),
                        HealthComponent = this.EntityManager.GetComponentData<HealthComponent>(playerEntity),
                        XPComponent = this.EntityManager.GetComponentData<XPComponent>(playerEntity),
                        HealthStatComponent = this.EntityManager.GetComponentData<HealthStatComponent>(playerEntity),
                        AttackStatComponent = this.EntityManager.GetComponentData<AttackStatComponent>(playerEntity),
                        DefenceStatComponent = this.EntityManager.GetComponentData<DefenceStatComponent>(playerEntity),
                        MagicStatComponent = this.EntityManager.GetComponentData<MagicStatComponent>(playerEntity)
                    };

                    playerData.ItemDataList = new List<ItemData>();
                    this.Entities.WithAll<ItemComponent, PickedUpComponent>().ForEach((Entity itemEntity,
                        ref ItemComponent itemComponent, ref PickedUpComponent pickedUpComponent) =>
                    {
                        if (pickedUpComponent.Owner == playerEntity)
                        {
                            playerData.ItemDataList.Add(new ItemData
                            {
                                IsEquiped = this.EntityManager.HasComponent<EquipedComponent>(itemEntity),
                                ItemComponent = itemComponent,
                                AttackModifier = this.EntityManager.HasComponent<AttackStatModifierComponent>(itemEntity) ?
                                                  this.EntityManager.GetComponentData<AttackStatModifierComponent>(itemEntity) :
                                                  new AttackStatModifierComponent(),
                                DefenceModifier = this.EntityManager.HasComponent<DefenceStatModifierComponent>(itemEntity) ?
                                                  this.EntityManager.GetComponentData<DefenceStatModifierComponent>(itemEntity) :
                                                  new DefenceStatModifierComponent(),
                                HealthModifier = this.EntityManager.HasComponent<HealthStatModifierComponent>(itemEntity) ?
                                                  this.EntityManager.GetComponentData<HealthStatModifierComponent>(itemEntity) :
                                                  new HealthStatModifierComponent(),
                                MagicModifier = this.EntityManager.HasComponent<MagicStatModifierComponent>(itemEntity) ?
                                                  this.EntityManager.GetComponentData<MagicStatModifierComponent>(itemEntity) :
                                                  new MagicStatModifierComponent(),
                            });
                        }
                    });

                    playerData.QuestDataList = new List<QuestData>();
                    this.Entities.WithAll<QuestComponent, QuestTextComponent>().ForEach((Entity questEntity,
                        ref QuestComponent questComponent, QuestTextComponent questTextComponent) =>
                    {
                        playerData.QuestDataList.Add(new QuestData
                        {
                            QuestText = questTextComponent.QuestText,
                            QuestComponent = questComponent,
                            LevelComponent = this.EntityManager.GetComponentData<LevelComponent>(questEntity),
                            XPRewardComponent = this.EntityManager.GetComponentData<XPRewardComponent>(questEntity),
                            IsDone = this.EntityManager.HasComponent<QuestDoneComponent>(questEntity),
                            IsInvestigateQuest = this.EntityManager.HasComponent<InvestigateQuestComponent>(questEntity),
                            IsDefeatQuest = this.EntityManager.HasComponent<DefeatQuestComponent>(questEntity),
                            IsLevelUpQuest = this.EntityManager.HasComponent<LevelUpQuestComponent>(questEntity),
                            IsLootQuest = this.EntityManager.HasComponent<LootQuestComponent>(questEntity),
                            IsReleaseQuest = this.EntityManager.HasComponent<ReleaseQuestComponent>(questEntity),
                            IsSpendQuest = this.EntityManager.HasComponent<SpendSkillPointQuestComponent>(questEntity),
                            IsPickUpQuest = this.EntityManager.HasComponent<PickUpQuestComponent>(questEntity),
                            PickUpQuestComponent = this.EntityManager.HasComponent<PickUpQuestComponent>(questEntity) ?
                                                    this.EntityManager.GetComponentData<PickUpQuestComponent>(questEntity) :
                                                    new PickUpQuestComponent()
                        });
                    });
                });
                if (playerData != null)
                {
                    SaveGameManager.SaveData(playerData);
                }

                this.PostUpdateCommands.DestroyEntity(entity);
            });
        }
    }
}
