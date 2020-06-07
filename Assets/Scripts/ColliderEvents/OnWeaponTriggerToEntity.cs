using BeyondPixels.ECS.Components.Characters.Common;

using Unity.Entities;

using UnityEngine;

namespace BeyondPixels.ColliderEvents
{
    public class OnWeaponTriggerToEntity : MonoBehaviour
    {
        public string TargetTag;
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("Hitbox")
                && collider.transform.parent.gameObject.CompareTag(this.TargetTag))
            {
                var entityManager = World.Active.EntityManager;
                var sender = this.GetComponentInParent<GameObjectEntity>().Entity;
                var target = collider.GetComponentInParent<GameObjectEntity>().Entity;
                if (!entityManager.Exists(sender) || !entityManager.Exists(target))
                {
                    return;
                }

                var eventEntity = entityManager.CreateEntity(typeof(CollisionInfo),
                                                             typeof(WeaponCollisionComponent),
                                                             typeof(DamageComponent));

                entityManager.SetComponentData(eventEntity,
                        new CollisionInfo
                        {
                            Sender = sender,
                            Target = target,
                            EventType = EventType.TriggerEnter
                        });
                entityManager.SetComponentData(eventEntity,
                        new DamageComponent
                        {
                            DamageType = DamageType.Weapon,
                            DamageOnImpact =
                                entityManager.GetComponentData<WeaponComponent>(sender).DamageValue
                        });
            }
        }
    }
}
