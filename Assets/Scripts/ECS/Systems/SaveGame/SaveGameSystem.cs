using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.SaveGame;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.SaveGame
{
    public class SaveGameSystem : ComponentSystem
    {
        private ComponentGroup _saveGroup;

        protected override void OnCreateManager()
        {
            _saveGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(SaveGameComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            Entities.With(_saveGroup).ForEach((Entity entity) =>
            {
                SaveData playerData = null;
                Entities.WithAll<PlayerComponent>().ForEach((Entity playerEntity) =>
                {
                    playerData = new SaveData
                    {
                        LevelComponent = EntityManager.GetComponentData<LevelComponent>(playerEntity),
                        HealthComponent = EntityManager.GetComponentData<HealthComponent>(playerEntity),
                        XPComponent = EntityManager.GetComponentData<XPComponent>(playerEntity),
                        HealthStatComponent = EntityManager.GetComponentData<HealthStatComponent>(playerEntity),
                        AttackStatComponent = EntityManager.GetComponentData<AttackStatComponent>(playerEntity),
                        DefenceStatComponent = EntityManager.GetComponentData<DefenceStatComponent>(playerEntity),
                        MagicStatComponent = EntityManager.GetComponentData<MagicStatComponent>(playerEntity)
                    };
                });
                if (playerData != null)
                    SaveGame(playerData);

                PostUpdateCommands.DestroyEntity(entity);
            });
        }

        private void SaveGame(SaveData playerSaveData)
        {
            var saveFolder = Path.Combine(Application.persistentDataPath, "SaveGame");
            var fileName = "savegame.save";
            var savePath = Path.Combine(saveFolder, fileName);
            Directory.CreateDirectory(saveFolder);

            if (File.Exists(savePath))
                File.Move(savePath, savePath + ".bckp");

            var binaryFormatter = new BinaryFormatter();

            using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                binaryFormatter.Serialize(fileStream, playerSaveData);

            File.Delete(savePath + ".bckp");
        }
    }
}
