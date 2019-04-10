using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.Scenes;
using BeyondPixels.ECS.Components.Spells;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.SceneManagement;

namespace BeyondPixels.ECS.Systems.Scenes
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class SceneLoadSystem : ComponentSystem
    {
        private ComponentGroup _sceneSwitchGroup;
        protected override void OnCreateManager()
        {
            _sceneSwitchGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(SceneLoadComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
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
