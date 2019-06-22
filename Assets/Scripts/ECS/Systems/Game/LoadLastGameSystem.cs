using BeyondPixels.ECS.Components.Game;
using BeyondPixels.ECS.Components.Scenes;
using Unity.Entities;
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
                    SceneIndex = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/DungeonScene.unity")
                });
            });
        }
    }
}
