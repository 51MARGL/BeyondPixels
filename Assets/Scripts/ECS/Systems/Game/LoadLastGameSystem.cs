using System;
using System.IO;
using BeyondPixels.ECS.Components.Game;
using BeyondPixels.ECS.Components.Scenes;
using Unity.Entities;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeyondPixels.ECS.Systems.Game
{
    public class LoadLastGameSystem : ComponentSystem
    {
        private ComponentGroup _loadGroup;

        protected override void OnCreateManager()
        {
            this._loadGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(LoadLastGameComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._loadGroup).ForEach((Entity entity) =>
            {
                var sceneLoadEntity = this.PostUpdateCommands.CreateEntity();
                this.PostUpdateCommands.AddComponent(sceneLoadEntity, new SceneLoadComponent
                {
                    SceneIndex = SceneManager.GetActiveScene().buildIndex
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
