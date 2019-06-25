using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.SceneBootstraps;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Common
{
    [UpdateAfter(typeof(DeathSystem))]
    public class DeathParticleEffectSystem : ComponentSystem
    {
        private EntityQuery _group;

        protected override void OnCreateManager()
        {
            this._group = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(HealthComponent),
                    typeof(PositionComponent), typeof(KilledComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            var positions = this._group.ToComponentDataArray<PositionComponent>(Allocator.TempJob);

            for (var i = 0; i < positions.Length; i++)
            {
                GameObject.Instantiate(PrefabManager.Instance.BloodDecalsPrefabs[UnityEngine.Random.Range(0, PrefabManager.Instance.BloodDecalsPrefabs.Length)],
                                       new float3(positions[i].CurrentPosition.x, positions[i].CurrentPosition.y, 0),
                                       Quaternion.identity);

            }
            positions.Dispose();
        }
    }
}
