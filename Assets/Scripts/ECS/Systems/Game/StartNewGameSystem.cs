using System;
using System.IO;
using BeyondPixels.ECS.Components.Game;
using BeyondPixels.ECS.Components.SaveGame;
using BeyondPixels.ECS.Components.Scenes;
using BeyondPixels.ECS.Systems.SaveGame;
using Unity.Entities;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeyondPixels.ECS.Systems.Game
{
    public class StartNewGameSystem : ComponentSystem
    {
        private EntityQuery _startGroup;

        protected override void OnCreateManager()
        {
            this._startGroup = this.GetEntityQuery(new EntityQueryDesc
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
                SaveGameManager.DeleteSave();

                var sceneLoadEntity = this.PostUpdateCommands.CreateEntity();
                this.PostUpdateCommands.AddComponent(sceneLoadEntity, new SceneLoadComponent
                {
                    SceneIndex = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/TutorialScene.unity")
                });
            });
        }
    }
}
