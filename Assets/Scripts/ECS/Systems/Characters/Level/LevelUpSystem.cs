using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.SceneBootstraps;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Level
{
    public class LevelUpSystem : ComponentSystem
    {
        private struct LevelUpProcessedComponent : IComponentData { }
        private ComponentGroup _characterGroup;
        private ComponentGroup _characterReadyGroup;

        protected override void OnCreateManager()
        {
            _characterGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(LevelComponent), typeof(LevelUpComponent),
                    typeof(PositionComponent), typeof(CharacterComponent)
                },
                None = new ComponentType[]
                {
                    typeof(LevelUpProcessedComponent)
                },
            });
            _characterReadyGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(LevelComponent), typeof(LevelUpComponent), typeof(LevelUpProcessedComponent)
                }
            });
        }
        protected override void OnUpdate()
        {
            if (_characterReadyGroup.CalculateLength() != 0)
            {
                Entities.With(_characterReadyGroup).ForEach((Entity entity) =>
                {
                    PostUpdateCommands.RemoveComponent<LevelUpProcessedComponent>(entity);
                    PostUpdateCommands.RemoveComponent<LevelUpComponent>(entity);
                });
            }
            if (_characterGroup.CalculateLength() != 0)
            {
                var positions = new NativeArray<float2>(_characterGroup.CalculateLength(), Allocator.TempJob);
                var k = 0;
                Entities.With(_characterGroup).ForEach((Entity entity, ref LevelComponent levelComponent, ref PositionComponent positionComponent, ref CharacterComponent characterComponent) =>
                {
                    if (characterComponent.CharacterType == CharacterType.Player)
                        positions[k++] = positionComponent.CurrentPosition;

                    levelComponent.CurrentLevel++;
                    levelComponent.NextLevelXP *= 2;

                    PostUpdateCommands.AddComponent(entity, new LevelUpProcessedComponent());
                });

                for (int i = 0; i < k; i++)
                    GameObject.Instantiate(PrefabManager.Instance.LevelUpEffectPrefab,
                                           new float3(positions[i].x, positions[i].y + 0.8f, 0),
                                           Quaternion.Euler(90, 0, 0));

                positions.Dispose();
            }
        }
    }
}
