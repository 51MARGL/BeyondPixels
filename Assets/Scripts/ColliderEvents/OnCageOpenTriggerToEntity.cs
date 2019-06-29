using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning;
using Unity.Entities;

using UnityEngine;

namespace BeyondPixels.ColliderEvents
{
    public class OnCageOpenTriggerToEntity : OnOpenTriggerToEntity
    {
        public override void OnAnimationEnd()
        {
            var entityManager = World.Active.EntityManager;
            var entity = this.GetComponent<GameObjectEntity>().Entity;
            entityManager.AddComponentData(entity, new CollectXPRewardComponent());
            Object.Destroy(this.GetComponent<CircleCollider2D>());

            var allySpawnEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(allySpawnEntity, new SpawnAllyComponent
            {
                Position = entityManager.GetComponentData<PositionComponent>(entity).CurrentPosition
            });
        }
    }
}
