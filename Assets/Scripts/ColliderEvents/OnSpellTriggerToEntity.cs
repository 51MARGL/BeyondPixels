﻿using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.ECS.Components.Spells;

using Unity.Entities;

using UnityEngine;

namespace BeyondPixels.ColliderEvents
{
    public class OnSpellTriggerToEntity : MonoBehaviour
    {
        private float totalTime;

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("Hitbox"))
            {
                var entityManager = World.Active.EntityManager;
                var spellEntity = this.GetComponent<GameObjectEntity>().Entity;
                if (!entityManager.Exists(spellEntity))
                {
                    return;
                }

                var eventEntity = entityManager.CreateEntity(typeof(CollisionInfo),
                                                             typeof(SpellCollisionComponent),
                                                             typeof(DamageComponent));

                var caster = entityManager.GetComponentData<SpellComponent>(spellEntity).Caster;
                var damageComponent = entityManager.GetComponentData<DamageComponent>(spellEntity);
                var targetComponent = entityManager.GetComponentData<TargetRequiredComponent>(spellEntity);

                entityManager.SetComponentData(eventEntity, damageComponent);
                entityManager.SetComponentData(eventEntity,
                         new CollisionInfo
                         {
                             Sender = caster,
                             Target = collider.GetComponentInParent<GameObjectEntity>().Entity,
                             EventType = EventType.TriggerEnter
                         });
                entityManager.SetComponentData(eventEntity,
                        new SpellCollisionComponent
                        {
                            Target = targetComponent.Target,
                            SpellEntity = spellEntity,
                            DestroyOnImpact = entityManager.HasComponent<DestroyOnImpactComponent>(spellEntity) ? 1 : 0
                        });
                this.totalTime = 0;
            }
            else if (collider.gameObject.CompareTag("Wall"))
            {
                var entityManager = World.Active.EntityManager;
                var spellEntity = this.GetComponent<GameObjectEntity>().Entity;
                if (!entityManager.Exists(spellEntity))
                {
                    return;
                }

                if (entityManager.HasComponent<ThrowOnTargetComponent>(spellEntity))
                {
                    entityManager.AddComponentData(spellEntity, new DestroyComponent());
                }
            }
        }

        private void OnTriggerStay2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("Hitbox"))
            {
                this.totalTime += Time.deltaTime;
                if (this.totalTime > 1f)
                {
                    var entityManager = World.Active.EntityManager;
                    var spellEntity = this.GetComponent<GameObjectEntity>().Entity;
                    if (!entityManager.Exists(spellEntity))
                    {
                        return;
                    }

                    var eventEntity = entityManager.CreateEntity(typeof(CollisionInfo),
                                                                 typeof(SpellCollisionComponent),
                                                                 typeof(DamageComponent));
                    var caster = entityManager.GetComponentData<SpellComponent>(spellEntity).Caster;
                    var damageComponent = entityManager.GetComponentData<DamageComponent>(spellEntity);
                    var targetComponent = entityManager.GetComponentData<TargetRequiredComponent>(spellEntity);

                    entityManager.SetComponentData(eventEntity, damageComponent);
                    entityManager.SetComponentData(eventEntity,
                            new CollisionInfo
                            {
                                Sender = caster,
                                Target = collider.GetComponentInParent<GameObjectEntity>().Entity,
                                EventType = EventType.TriggerStay
                            });
                    entityManager.SetComponentData(eventEntity,
                            new SpellCollisionComponent
                            {
                                Target = targetComponent.Target,
                            });
                    this.totalTime = 0;
                }
            }
        }
    }
}
