using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Items;
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
            this._saveGroup = this.GetComponentGroup(new EntityArchetypeQuery
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
                            playerData.ItemDataList.Add(new ItemData
                            {
                                IsEquiped = EntityManager.HasComponent<EquipedComponent>(itemEntity),
                                ItemComponent = itemComponent,
                                AttackModifier =  EntityManager.HasComponent<AttackStatModifierComponent>(itemEntity) ?
                                                  EntityManager.GetComponentData<AttackStatModifierComponent>(itemEntity) :
                                                  new AttackStatModifierComponent(),
                                DefenceModifier = EntityManager.HasComponent<DefenceStatModifierComponent>(itemEntity) ?
                                                  EntityManager.GetComponentData<DefenceStatModifierComponent>(itemEntity) :
                                                  new DefenceStatModifierComponent(),
                                HealthModifier =  EntityManager.HasComponent<HealthStatModifierComponent>(itemEntity) ?
                                                  EntityManager.GetComponentData<HealthStatModifierComponent>(itemEntity) :
                                                  new HealthStatModifierComponent(),
                                MagicModifier =   EntityManager.HasComponent<MagickStatModifierComponent>(itemEntity) ?
                                                  EntityManager.GetComponentData<MagickStatModifierComponent>(itemEntity) :
                                                  new MagickStatModifierComponent(),
                            });
                    });
                });
                if (playerData != null)
                    this.SaveGame(playerData);

                this.PostUpdateCommands.DestroyEntity(entity);
            });
        }

        private void SaveGame(SaveData playerSaveData)
        {
            var saveFolder = Path.Combine(Application.persistentDataPath, "SaveGame");
            var fileName = "savegame.save";
            var savePath = Path.Combine(saveFolder, fileName);
            var saveBckpPath = savePath + Guid.NewGuid() + ".bckp";

            try
            {
                Directory.CreateDirectory(saveFolder);

                if (File.Exists(savePath))
                    File.Move(savePath, saveBckpPath);

                var binaryFormatter = new BinaryFormatter();

                using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    binaryFormatter.Serialize(fileStream, playerSaveData);

                File.Delete(saveBckpPath);
            }
            catch (Exception)
            {
                if (File.Exists(saveBckpPath))
                    File.Move(saveBckpPath, savePath);
            }
        }
    }
}
