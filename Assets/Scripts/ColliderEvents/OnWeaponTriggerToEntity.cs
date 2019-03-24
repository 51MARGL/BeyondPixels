using BeyondPixels.ECS.Components.Characters.Common;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ColliderEvents
{
    public class OnWeaponTriggerToEntity : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("Hitbox")
                && !this.transform.parent.gameObject.CompareTag(collider.transform.parent.tag))
            {
                var entityManager = World.Active.GetExistingManager<EntityManager>();
                var sender = GetComponentInParent<GameObjectEntity>().Entity;
                var target = collider.GetComponentInParent<GameObjectEntity>().Entity;
                if (!entityManager.Exists(sender) || !entityManager.Exists(target))
                    return;

                var eventEntity = entityManager.CreateEntity(typeof(CollisionInfo), typeof(FinalDamageComponent));

                entityManager.SetComponentData(eventEntity,
                        new CollisionInfo
                        {
                            Sender = sender,
                            Target = target,
                            EventType = EventType.TriggerEnter
                        });
                entityManager.SetComponentData(eventEntity,
                        new FinalDamageComponent
                        {
                            DamageType = DamageType.Weapon,
                            DamageAmount =
                                entityManager.GetComponentData<WeaponComponent>(sender).DamageValue
                        });
            }
        }
    }
}
