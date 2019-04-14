using BeyondPixels.ECS.Components.SaveGame;
using BeyondPixels.ECS.Components.Scenes;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.SceneManagement;

namespace BeyondPixels.ECS.Systems.Scenes
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class SceneLoadSystem : ComponentSystem
    {
        private ComponentGroup _sceneSwitchGroup;
        private ComponentGroup _saveGameGroup;
        protected override void OnCreateManager()
        {
            _sceneSwitchGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(SceneLoadComponent)
                }
            });
            _saveGameGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(SaveGameComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            if (_saveGameGroup.CalculateLength() > 0)
                return;

            Entities.With(_sceneSwitchGroup).ForEach((Entity entity, ref SceneLoadComponent sceneLoadComponent) =>
            {
                SceneManager.LoadScene(sceneLoadComponent.SceneIndex, LoadSceneMode.Single);
            });
            if (_sceneSwitchGroup.CalculateLength() > 0)
            {
                var entityArray = EntityManager.GetAllEntities(Allocator.TempJob);
                for (int i = 0; i < entityArray.Length; i++)
                    PostUpdateCommands.DestroyEntity(entityArray[i]);
                entityArray.Dispose();
            }
        }
    }
}
