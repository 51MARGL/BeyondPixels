using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Items;

using Unity.Entities;

using UnityEngine;

namespace BeyondPixels.ColliderEvents
{
    public class OnChestOpenTriggerToEntity : OnOpenTriggerToEntity
    {
        public override void OnAnimationEnd()
        {
            var entityManager = World.Active.EntityManager;
            var entity = this.GetComponent<GameObjectEntity>().Entity;
            entityManager.AddComponentData(entity, new DropLootComponent());
            entityManager.AddComponentData(entity, new CollectXPRewardComponent());
            Object.Destroy(this.GetComponent<CircleCollider2D>());
        }
    }
}
