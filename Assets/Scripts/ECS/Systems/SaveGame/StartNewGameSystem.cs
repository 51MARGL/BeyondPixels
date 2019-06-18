using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.SaveGame;
using BeyondPixels.ECS.Components.Scenes;
using Unity.Entities;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeyondPixels.ECS.Systems.SaveGame
{
    public class StartNewGameSystem : ComponentSystem
    {
        private ComponentGroup _startGroup;

        protected override void OnCreateManager()
        {
            this._startGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(StartNewGameComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._startGroup).ForEach((Entity entity) =>
            {
                this.DeleteSave();
                var sceneLoadEntity = this.PostUpdateCommands.CreateEntity();
                this.PostUpdateCommands.AddComponent(sceneLoadEntity, new SceneLoadComponent
                {
                    SceneIndex = SceneManager.GetSceneByName("DungeonScene").buildIndex
                });
            });
        }

        private void DeleteSave()
        {
            var saveFolder = Path.Combine(Application.persistentDataPath, "SaveGame");
            var fileName = "savegame.save";
            var savePath = Path.Combine(saveFolder, fileName);

            if (File.Exists(savePath))
            {
                try
                {
                    File.Delete(savePath);
                }
                catch (Exception) { }
            }
        }
    }
}
