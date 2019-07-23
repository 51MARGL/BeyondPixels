using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.Quest;
using BeyondPixels.ECS.Components.SaveGame;
using BeyondPixels.SceneBootstraps;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.SaveGame
{
    public class LoadGameSystem : ComponentSystem
    {
        private EntityQuery _loadGroup;
        private EntityQuery _playerGroup;

        protected override void OnCreate()
        {
            this._loadGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(LoadGameComponent)
                }
            });
            this._playerGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(PlayerComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            var loadEntities = this._loadGroup.ToEntityArray(Allocator.TempJob);
            for (var e = 0; e < loadEntities.Length; e++)
            {
                if (SaveGameManager.LoadData() is SaveData playerData)
                {
                    var playerEntities = this._playerGroup.ToEntityArray(Allocator.TempJob);
                    for (var p = 0; p < playerEntities.Length; p++)
                    {
                        var playerEntity = playerEntities[p];

                        this.PostUpdateCommands.SetComponent(playerEntity, playerData.LevelComponent);
                        this.PostUpdateCommands.SetComponent(playerEntity, playerData.HealthComponent);
                        this.PostUpdateCommands.SetComponent(playerEntity, playerData.XPComponent);
                        this.PostUpdateCommands.SetComponent(playerEntity, playerData.HealthStatComponent);
                        this.PostUpdateCommands.SetComponent(playerEntity, playerData.AttackStatComponent);
                        this.PostUpdateCommands.SetComponent(playerEntity, playerData.DefenceStatComponent);
                        this.PostUpdateCommands.SetComponent(playerEntity, playerData.MagicStatComponent);

                        if (playerData.ItemDataList != null)
                            for (var i = 0; i < playerData.ItemDataList.Count; i++)
                                this.LoadItem(playerData.ItemDataList[i], playerEntity);

                        if (playerData.QuestDataList != null)
                        {
                            for (var i = 0; i < playerData.QuestDataList.Count; i++)
                                this.LoadQuest(playerData.QuestDataList[i]);

                            for (var i = 0; i < 5 - playerData.QuestDataList.Count; i++)
                            {
                                var genQuestEntity = this.PostUpdateCommands.CreateEntity();
                                this.PostUpdateCommands.AddComponent(genQuestEntity, new GenerateQuestComponent());
                            }
                        }                        
                    }
                    playerEntities.Dispose();
                }
                this.PostUpdateCommands.DestroyEntity(loadEntities[e]);
            }
            loadEntities.Dispose();
        }

        private void LoadItem(ItemData itemData, Entity playerEntity)
        {
            var itemEntity = this.PostUpdateCommands.CreateEntity();
            var pickedUpComponent = new PickedUpComponent
            {
                Owner = playerEntity
            };
            this.PostUpdateCommands.AddComponent(itemEntity, itemData.ItemComponent);
            this.PostUpdateCommands.AddComponent(itemEntity, pickedUpComponent);
            if (itemData.IsEquiped)
                this.PostUpdateCommands.AddComponent(itemEntity, new EquipedComponent());

            if (itemData.AttackModifier.Value > 0)
                this.PostUpdateCommands.AddComponent(itemEntity, itemData.AttackModifier);
            if (itemData.DefenceModifier.Value > 0)
                this.PostUpdateCommands.AddComponent(itemEntity, itemData.DefenceModifier);
            if (itemData.HealthModifier.Value > 0)
                this.PostUpdateCommands.AddComponent(itemEntity, itemData.HealthModifier);
            if (itemData.MagicModifier.Value > 0)
                this.PostUpdateCommands.AddComponent(itemEntity, itemData.MagicModifier);
        }

        private void LoadQuest(QuestData questData)
        {
            var questObj = GameObject.Instantiate(PrefabManager.Instance.QuestPrefab);
            var questEntity = questObj.GetComponent<GameObjectEntity>().Entity;

            questObj.GetComponent<QuestTextComponent>().QuestText = questData.QuestText;
            this.PostUpdateCommands.AddComponent(questEntity, questData.QuestComponent);
            this.PostUpdateCommands.AddComponent(questEntity, questData.LevelComponent);
            this.PostUpdateCommands.AddComponent(questEntity, questData.XPRewardComponent);
            this.PostUpdateCommands.AddComponent(questEntity, new LevelAdjustedComponent());
            if (questData.IsPickUpQuest)
                this.PostUpdateCommands.AddComponent(questEntity, questData.PickUpQuestComponent);
            if (questData.IsDefeatQuest)
                this.PostUpdateCommands.AddComponent(questEntity, new DefeatQuestComponent());
            if (questData.IsInvestigateQuest)
                this.PostUpdateCommands.AddComponent(questEntity, new InvestigateQuestComponent());
            if (questData.IsLevelUpQuest)
                this.PostUpdateCommands.AddComponent(questEntity, new LevelUpQuestComponent());
            if (questData.IsLootQuest)
                this.PostUpdateCommands.AddComponent(questEntity, new LootQuestComponent());
            if (questData.IsReleaseQuest)
                this.PostUpdateCommands.AddComponent(questEntity, new ReleaseQuestComponent());
            if (questData.IsSpendQuest)
                this.PostUpdateCommands.AddComponent(questEntity, new SpendSkillPointQuestComponent());
            if (questData.IsDone)
                this.PostUpdateCommands.AddComponent(questEntity, new QuestDoneComponent());
        }
    }
}
