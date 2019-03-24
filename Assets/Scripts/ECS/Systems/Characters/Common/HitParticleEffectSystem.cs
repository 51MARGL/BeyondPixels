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
        private ComponentGroup _damageGroup;
        private ComponentGroup _characterGroup;

        protected override void OnCreateManager()
        {
            _damageGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(CollisionInfo), typeof(FinalDamageComponent)
                }
            });
            _characterGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(CharacterComponent), typeof(PositionComponent)
                }
            });
        }
        protected override void OnUpdate()
        {
            var positions = new NativeArray<float2>(_damageGroup.CalculateLength(), Allocator.TempJob);

            var k = 0;
            Entities.With(_damageGroup).ForEach((Entity damageEntity, ref FinalDamageComponent damageComponent, ref CollisionInfo collisionInfo) =>
            {
                var eventTarget = collisionInfo.Target;
                var damageValue = damageComponent.DamageAmount;
                Entities.With(_characterGroup).ForEach((Entity characterEntity, ref PositionComponent positionComponent) =>
                {
                    if (eventTarget == characterEntity && damageValue > 0)
                        positions[k++] = positionComponent.CurrentPosition;
                });
            });

            for (int i = 0; i < k; i++)
                GameObject.Instantiate(PrefabManager.Instance.BloodSplashPrefab,
                                       new float3(positions[i].x, positions[i].y, -1),
                                       Quaternion.identity);
            positions.Dispose();
        }
    }
}