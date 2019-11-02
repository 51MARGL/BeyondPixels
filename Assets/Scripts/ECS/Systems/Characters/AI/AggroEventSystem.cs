using BeyondPixels.ColliderEvents;
using BeyondPixels.ECS.Components.Characters.AI;

using Unity.Collections;
using Unity.Entities;

namespace BeyondPixels.ECS.Systems.Characters.AI
{
    public class AggroEventSystem : ComponentSystem
    {
        private EntityQuery _targetGroup;

        protected override void OnCreate()
        {
            this._targetGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(CollisionInfo), typeof(AggroRangeCollisionComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            var senderTargetSet = new NativeHashMap<Entity, Entity>(this._targetGroup.CalculateEntityCount(), Allocator.Temp);
            this.Entities.With(this._targetGroup).ForEach((Entity eventEntity, ref CollisionInfo collisionInfo) =>
            {
                switch (collisionInfo.EventType)
                {
                    case EventType.TriggerEnter:
                        if (!this.EntityManager.HasComponent<FollowStateComponent>(collisionInfo.Sender))
                            senderTargetSet.TryAdd(collisionInfo.Sender, collisionInfo.Target);
                        break;
                    case EventType.TriggerExit:
                        if (this.EntityManager.HasComponent<FollowStateComponent>(collisionInfo.Sender))
                        {
                            var followStateComponent = this.EntityManager.GetComponentData<FollowStateComponent>(collisionInfo.Sender);
                            if (followStateComponent.Target == collisionInfo.Target)
                                this.PostUpdateCommands.RemoveComponent<FollowStateComponent>(collisionInfo.Sender);
                        }
                        break;
                }

                this.PostUpdateCommands.DestroyEntity(eventEntity);
            });
            if (senderTargetSet.Length > 0)
            {
                var keys = senderTargetSet.GetKeyArray(Allocator.Temp);
                for (var i = 0; i < keys.Length; i++)
                {
                    if (senderTargetSet.TryGetValue(keys[i], out var target))
                        this.PostUpdateCommands.AddComponent(keys[i], new FollowStateComponent
                        {
                            Target = target
                        });
                }
            }
            senderTargetSet.Dispose();
        }
    }
}
