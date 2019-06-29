using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Spells;
using BeyondPixels.ECS.Systems.Characters.Common;
using BeyondPixels.Utilities;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Player
{
    [UpdateAfter(typeof(MovementSystem))]
    [UpdateInGroup(typeof(FixedUpdateSystemGroup))]
    public class LookAtTargetSystem : ComponentSystem
    {
        private EntityQuery _group;

        protected override void OnCreateManager()
        {
            this._group = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly(typeof(MovementComponent)),
                    typeof(UnityEngine.Transform),
                    typeof(SpellCastingComponent),
                    typeof(TargetComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._group).ForEach((Entity entity, Transform transform, ref SpellCastingComponent spellCastingComponent, ref TargetComponent targetComponent) =>
            {
                if (!EntityManager.Exists(targetComponent.Target))
                    return;

                var spell = SpellBookManagerComponent.Instance.SpellBook.Spells[spellCastingComponent.SpellIndex];
                var targetPosition = EntityManager.GetComponentObject<Transform>(targetComponent.Target).position;
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