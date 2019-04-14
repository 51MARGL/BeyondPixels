using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using BeyondPixels.ECS.Components.Characters.Player;
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
            _loadGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(LoadGameComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            Entities.With(_loadGroup).ForEach((Entity entity) =>
            {
                var playerData = LoadGame();
                if (playerData != null)
                    Entities.WithAll<PlayerComponent>().ForEach((Entity playerEntity) =>
                    {
                        PostUpdateCommands.SetComponent(playerEntity, playerData.LevelComponent);
                        PostUpdateCommands.SetComponent(playerEntity, playerData.HealthComponent);
                        PostUpdateCommands.SetComponent(playerEntity, playerData.XPComponent);
                        PostUpdateCommands.SetComponent(playerEntity, playerData.HealthComponent);
                        PostUpdateCommands.SetComponent(playerEntity, playerData.AttackStatComponent);
                        PostUpdateCommands.SetComponent(playerEntity, playerData.DefenceStatComponent);
                        PostUpdateCommands.SetComponent(playerEntity, playerData.MagicStatComponent);
                    });
                PostUpdateCommands.DestroyEntity(entity);
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
