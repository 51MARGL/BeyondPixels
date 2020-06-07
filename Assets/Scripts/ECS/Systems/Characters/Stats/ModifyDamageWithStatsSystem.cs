using BeyondPixels.ColliderEvents;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Systems.Characters.Common;

using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Systems.Characters.Stats
{
    [UpdateBefore(typeof(DamageSystem))]
    public class ModifyDamageWithStatsSystem : ComponentSystem
    {
        private EntityQuery _group;

        protected override void OnCreate()
        {
            this._group = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(CollisionInfo), typeof(FinalDamageComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._group).ForEach((ref CollisionInfo collisionInfo, ref FinalDamageComponent finalDamageComponent) =>
            {
                if (this.EntityManager.Exists(collisionInfo.Sender) && this.EntityManager.Exists(collisionInfo.Target))
                {
                    switch (finalDamageComponent.DamageType)
                    {
                        case DamageType.Weapon:
                            var attackStatComponent = this.EntityManager.GetComponentData<AttackStatComponent>(collisionInfo.Sender);
                            var defenceStatComponent = this.EntityManager.GetComponentData<DefenceStatComponent>(collisionInfo.Target);
                            var attackModifier = finalDamageComponent.DamageAmount / 100f * attackStatComponent.CurrentValue;
                            var defenceModifier = finalDamageComponent.DamageAmount / 100f * defenceStatComponent.CurrentValue;
                            finalDamageComponent.DamageAmount += math.max(0, attackModifier - defenceModifier);
                            break;
                        case DamageType.Magic:
                            var casterMagicStatComponent = this.EntityManager.GetComponentData<MagicStatComponent>(collisionInfo.Sender);
                            var targetMagicStatComponent = this.EntityManager.GetComponentData<MagicStatComponent>(collisionInfo.Target);
                            var casterMagicModifier = finalDamageComponent.DamageAmount / 100f * casterMagicStatComponent.CurrentValue;
                            var targetMagicModifier = finalDamageComponent.DamageAmount / 100f * targetMagicStatComponent.CurrentValue;
                            if (collisionInfo.Sender == collisionInfo.Target)
                            {
                                finalDamageComponent.DamageAmount += casterMagicModifier;
                            }
                            else
                            {
                                finalDamageComponent.DamageAmount += math.max(0, casterMagicModifier - targetMagicModifier);
                            }

                            break;
                    }
                }
            });
        }
    }
}
