using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.SaveGame;

using Unity.Entities;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.SaveGame
{
    public class LoadGameSystem : ComponentSystem
    {
        private ComponentGroup _loadGroup;

        protected override void OnCreateManager()
        {
            this._loadGroup = this.GetComponentGroup(new EntityArchetypeQuery
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
                var playerData = this.LoadGame();
                if (playerData != null)
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
                            }
                    });
                this.PostUpdateCommands.DestroyEntity(entity);
            });
        }

        private SaveData LoadGame()
        {
            SaveData playerData = null;
            var saveFolder = Path.Combine(Application.persistentDataPath, "SaveGame");
            var fileName = "savegame.save";
            var savePath = Path.Combine(saveFolder, fileName);

            if (!File.Exists(savePath))
                return playerData;

            var binaryFormatter = new BinaryFormatter();

            using (var fileStream = new FileStream(savePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                playerData = (SaveData)binaryFormatter.Deserialize(fileStream);

            return playerData;
        }
    }
}
