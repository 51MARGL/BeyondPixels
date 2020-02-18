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
            var count = this._damageGroup.CalculateEntityCount();
            if (count == 0)
            {
                return;
            }

            var positions = new NativeArray<PositionComponent>(count, Allocator.TempJob);
            var senders = new NativeArray<Entity>(count, Allocator.TempJob);
            var damageTypes = new NativeArray<DamageType>(count, Allocator.TempJob);

            var k = 0;
            this.Entities.With(this._damageGroup).ForEach((Entity damageEntity, ref FinalDamageComponent damageComponent, ref CollisionInfo collisionInfo) =>
            {
                if (damageComponent.DamageAmount > 0)
                {
                    positions[k] = this.EntityManager.GetComponentData<PositionComponent>(collisionInfo.Target);
                    senders[k] = collisionInfo.Sender;
                    damageTypes[k++] = damageComponent.DamageType;
                }
            });

            for (var i = 0; i < k; i++)
            {
                var senderPosition = float2.zero;
                if (this.EntityManager.Exists(senders[i]))
                {
                    senderPosition = this.EntityManager.GetComponentData<PositionComponent>(senders[i]).CurrentPosition;
                }

                var destination = positions[i].CurrentPosition;
                if (damageTypes[i] != DamageType.Weapon)
                {
                    destination.y += 0.25f;
                }

                var obj = GameObject.Instantiate(PrefabManager.Instance.BloodSplashPrefab,
                                        new float3(destination.x, destination.y, 0f),
                                        Quaternion.identity);

                obj.transform.right = Vector3.down;

                if (damageTypes[i] == DamageType.Weapon && !senderPosition.Equals(float2.zero))
                {
                    obj.transform.right = obj.transform.position -
                        new Vector3(senderPosition.x, senderPosition.y, 0f);
                }
            }
            positions.Dispose();
            senders.Dispose();
            damageTypes.Dispose();
        }
    }
}