using BeyondPixels.ColliderEvents;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.SceneBootstraps;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Common
{
    [UpdateBefore(typeof(DamageSystem))]
    public class HitParticleEffectSystem : ComponentSystem
    {
        private EntityQuery _damageGroup;
        private EntityQuery _characterGroup;

        protected override void OnCreate()
        {
            this._damageGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(CollisionInfo), typeof(FinalDamageComponent)
                }
            });
            this._characterGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(CharacterComponent), typeof(PositionComponent)
                }
            });
        }
        protected override void OnUpdate()
        {
            if (this._damageGroup.CalculateLength() == 0)
                return;

            var positions = new NativeArray<PositionComponent>(this._damageGroup.CalculateLength(), Allocator.TempJob);

            var k = 0;
            this.Entities.With(this._damageGroup).ForEach((Entity damageEntity, ref FinalDamageComponent damageComponent, ref CollisionInfo collisionInfo) =>
            {
                if (damageComponent.DamageAmount > 0)
                    positions[k++] = EntityManager.GetComponentData<PositionComponent>(collisionInfo.Target);
            });

            for (var i = 0; i < k; i++)
            {
                GameObject.Instantiate(PrefabManager.Instance.BloodSplashPrefab,
                                       new float3(positions[i].CurrentPosition.x, positions[i].CurrentPosition.y, -1),
                                       Quaternion.identity);
            }
            positions.Dispose();
        }
    }
}