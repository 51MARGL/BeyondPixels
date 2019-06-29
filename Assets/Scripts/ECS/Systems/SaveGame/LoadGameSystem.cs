using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.SaveGame;

using Unity.Entities;

namespace BeyondPixels.ECS.Systems.SaveGame
{
    public class LoadGameSystem : ComponentSystem
    {
        private EntityQuery _loadGroup;

        protected override void OnCreate()
        {
            this._loadGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(LoadGameComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._loadGroup).ForEach((Entity entity) =>
            {
                if (SaveGameManager.LoadData() is SaveData playerData)
                {
                    this.Entities.WithAll<PlayerComponent>().ForEach((Entity playerEntity) =>
                    {
                        this.PostUpdateCommands.SetComponent(playerEntity, playerData.LevelComponent);
                        this.PostUpdateCommands.SetComponent(playerEntity, playerData.HealthComponent);
                        this.PostUpdateCommands.SetComponent(playerEntity, playerData.XPComponent);
                        this.PostUpdateCommands.SetComponent(playerEntity, playerData.HealthStatComponent);
                        this.PostUpdateCommands.SetComponent(playerEntity, playerData.AttackStatComponent);
                        this.PostUpdateCommands.SetComponent(playerEntity, playerData.DefenceStatComponent);
                        this.PostUpdateCommands.SetComponent(playerEntity, playerData.MagicStatComponent);

                        if (playerData.ItemDataList != null)
                            for (var i = 0; i < playerData.ItemDataList.Count; i++)
                            {
                                var itemEntity = this.PostUpdateCommands.CreateEntity();
                                var pickedUpComponent = new PickedUpComponent
                                {
                                    Owner = playerEntity
                                };
                                this.PostUpdateCommands.AddComponent(itemEntity, playerData.ItemDataList[i].ItemComponent);
                                this.PostUpdateCommands.AddComponent(itemEntity, pickedUpComponent);
                                if (playerData.ItemDataList[i].IsEquiped)
                                    this.PostUpdateCommands.AddComponent(itemEntity, new EquipedComponent());

                                if (playerData.ItemDataList[i].AttackModifier.Value > 0)
                                    this.PostUpdateCommands.AddComponent(itemEntity, playerData.ItemDataList[i].AttackModifier);
                                if (playerData.ItemDataList[i].DefenceModifier.Value > 0)
                                    this.PostUpdateCommands.AddComponent(itemEntity, playerData.ItemDataList[i].DefenceModifier);
                                if (playerData.ItemDataList[i].HealthModifier.Value > 0)
                                    this.PostUpdateCommands.AddComponent(itemEntity, playerData.ItemDataList[i].HealthModifier);
                                if (playerData.ItemDataList[i].MagicModifier.Value > 0)
                                    this.PostUpdateCommands.AddComponent(itemEntity, playerData.ItemDataList[i].MagicModifier);
                            }
                    });
                }
                this.PostUpdateCommands.DestroyEntity(entity);
            });
        }
    }
}
