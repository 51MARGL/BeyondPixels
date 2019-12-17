using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Systems.Characters.Common;
using BeyondPixels.Utilities;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.AI
{
    [UpdateAfter(typeof(MovementSystem))]
    [UpdateInGroup(typeof(FixedUpdateSystemGroup))]
    public class LookAtTargetSystem : ComponentSystem
    {
        private EntityQuery _group;

        protected override void OnCreate()
        {
            this._group = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly(typeof(MovementComponent)),
                    typeof(UnityEngine.Transform),
                    typeof(SpellCastingComponent),
                    typeof(FollowStateComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._group).ForEach((Entity entity,
                                                    Transform transform,
                                                    ref FollowStateComponent followStateComponent) =>
            {
                if (!this.EntityManager.Exists(followStateComponent.Target))
                    return;
                
                var targetPosition = this.EntityManager.GetComponentObject<Transform>(followStateComponent.Target).position;
                var direction = targetPosition - transform.position;
                var scale = math.abs(transform.localScale.x);

                if (direction.x < 0f)
                    transform.localScale = new Vector3(-scale, transform.localScale.y, transform.localScale.z);
                else if (direction.x > 0f)
                    transform.localScale = new Vector3(scale, transform.localScale.y, transform.localScale.z);
            });
        }
    }
}