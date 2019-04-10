using BeyondPixels.ColliderEvents;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Systems.Characters.Common;
using Unity.Entities;

namespace BeyondPixels.ECS.Systems.Characters.Stats
{
    [UpdateBefore(typeof(DamageSystem))]
    public class ModifyDamageWithStatsSystem : ComponentSystem
    {
        private ComponentGroup _group;

        protected override void OnCreateManager()
        {
            _group = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(CollisionInfo), typeof(FinalDamageComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            Entities.With(_group).ForEach((ref CollisionInfo collisionInfo, ref FinalDamageComponent finalDamageComponent) =>
            {
                switch (finalDamageComponent.DamageType)
                {
                    case DamageType.Weapon:
                        var attackStatComponent = EntityManager.GetComponentData<AttackStatComponent>(collisionInfo.Sender);
                        var defenceStatComponent = EntityManager.GetComponentData<DefenceStatComponent>(collisionInfo.Target);
                        var attackModifier = finalDamageComponent.DamageAmount / 100f * attackStatComponent.CurrentValue;
                        var defenceModifier = finalDamageComponent.DamageAmount / 100f * defenceStatComponent.CurrentValue;
                        finalDamageComponent.DamageAmount += attackModifier - defenceModifier;
                        break;
                    case DamageType.Magic:
                        var casterMagicStatComponent = EntityManager.GetComponentData<MagicStatComponent>(collisionInfo.Sender);
                        var targetMagicStatComponent = EntityManager.GetComponentData<MagicStatComponent>(collisionInfo.Target);
                        var casterMagicModifier = finalDamageComponent.DamageAmount / 100f * casterMagicStatComponent.CurrentValue;
                        var targetMagicModifier = finalDamageComponent.DamageAmount / 100f * targetMagicStatComponent.CurrentValue;
                        if (collisionInfo.Sender == collisionInfo.Target)
                            finalDamageComponent.DamageAmount += casterMagicModifier;
                        else
                            finalDamageComponent.DamageAmount += casterMagicModifier - targetMagicModifier;
                        break;
                }
            });
        }
    }
}
