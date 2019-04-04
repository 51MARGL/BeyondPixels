using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.SceneBootstraps;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Common
{
    public class DeathSystem : ComponentSystem
    {       
        private ComponentGroup _group;

        protected override void OnCreateManager()
        {
            _group = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(HealthComponent), typeof(CharacterComponent),
                    typeof(PositionComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            if (_group.CalculateLength() == 0)
                return;

            var positions = new NativeArray<float2>(_group.CalculateLength(), Allocator.TempJob);

            var k = 0;
            Entities.With(_group).ForEach((Entity entity, ref HealthComponent healthComponent, ref PositionComponent positionComponent) =>
            {
                if (healthComponent.CurrentValue <= 0)
                {
                    PostUpdateCommands.AddComponent(entity, new DestroyComponent());
                    positions[k++] = positionComponent.CurrentPosition;
                }
            });

            for (int i = 0; i < k; i++)
            {
                GameObject.Instantiate(PrefabManager.Instance.BloodDecalsPrefabs[UnityEngine.Random.Range(0, PrefabManager.Instance.BloodDecalsPrefabs.Length)],
                                       new float3(positions[i].x, positions[i].y, 0),
                                       Quaternion.identity);

            }
            positions.Dispose();
        }
    }
}
