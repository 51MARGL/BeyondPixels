using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Systems.Characters.Stats;
using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Systems.Items
{
    [UpdateAfter(typeof(AfterStatsAdjustSystem))]
    public class ModifyStatsWithItemsSystem : ComponentSystem
    {
        private EntityQuery _gearGroup;
        private EntityQuery _ownerGroup;

        protected override void OnCreateManager()
        {
            this._gearGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(ItemComponent), typeof(EquipedComponent), typeof(PickedUpComponent)
                }
            });
            this._ownerGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(HealthStatComponent),typeof(AttackStatComponent),
                    typeof(DefenceStatComponent), typeof(MagicStatComponent), typeof(HealthComponent)
                },
                None = new ComponentType[]
                {
                    typeof(AddStatPointComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._ownerGroup).ForEach((Entity characterEntity,
                ref HealthStatComponent healthStatComponent,
                ref AttackStatComponent attackStatComponent,
                ref DefenceStatComponent defenceStatComponent,
                ref MagicStatComponent magicStatComponent,
                ref HealthComponent healthComponent) =>
            {
                var attackModifier = 0;
                var defenceModifier = 0;
                var healthModifier = 0;
                var magicModifier = 0;

                this.Entities.With(this._gearGroup).ForEach((Entity gearEntity, ref ItemComponent itemComponent, ref PickedUpComponent pickedUpComponent) =>
                {
                    if (characterEntity != pickedUpComponent.Owner)
                        return;

                    var itemOwner = pickedUpComponent.Owner;
                    var item = ItemsManagerComponent.Instance.ItemsStoreComponent.Items[itemComponent.StoreIndex];
                    if (item.ItemType == ItemType.Gear)
                    {
                        if (this.EntityManager.HasComponent<AttackStatModifierComponent>(gearEntity))
                        {
                            var modifierComponent = this.EntityManager.GetComponentData<AttackStatModifierComponent>(gearEntity);
                            attackModifier += modifierComponent.Value * itemComponent.Level;
                        }
                        if (this.EntityManager.HasComponent<DefenceStatModifierComponent>(gearEntity))
                        {
                            var modifierComponent = this.EntityManager.GetComponentData<DefenceStatModifierComponent>(gearEntity);
                            defenceModifier += modifierComponent.Value * itemComponent.Level;

                        }
                        if (this.EntityManager.HasComponent<HealthStatModifierComponent>(gearEntity))
                        {
                            var modifierComponent = this.EntityManager.GetComponentData<HealthStatModifierComponent>(gearEntity);
                            healthModifier += modifierComponent.Value * itemComponent.Level;
                        }
                        if (this.EntityManager.HasComponent<MagickStatModifierComponent>(gearEntity))
                        {
                            var modifierComponent = this.EntityManager.GetComponentData<MagickStatModifierComponent>(gearEntity);
                            magicModifier += modifierComponent.Value * itemComponent.Level;
                        }
                    }
                });

                var properValue = (attackStatComponent.BaseValue
                                  + attackStatComponent.PerPointValue
                                  * (attackStatComponent.PointsSpent - 1)) + attackModifier;
                if (attackStatComponent.CurrentValue != properValue)
                    attackStatComponent.CurrentValue = properValue;


                properValue = (defenceStatComponent.BaseValue
                                  + defenceStatComponent.PerPointValue
                                  * (defenceStatComponent.PointsSpent - 1)) + defenceModifier;
                if (defenceStatComponent.CurrentValue != properValue)
                    defenceStatComponent.CurrentValue = properValue;

                properValue = (healthStatComponent.BaseValue
                                  + healthStatComponent.PerPointValue
                                  * (healthStatComponent.PointsSpent - 1)) + healthModifier;
                if (healthStatComponent.CurrentValue != properValue)
                {
                    healthStatComponent.CurrentValue = properValue;

                    healthComponent.MaxValue = healthComponent.BaseValue
                        + (healthComponent.BaseValue / 100f * properValue * math.log2(properValue));

                    if (healthComponent.CurrentValue > healthComponent.MaxValue)
                        healthComponent.CurrentValue = healthComponent.MaxValue;

                    if (healthComponent.MaxValue > healthComponent.CurrentValue 
                        && EntityManager.HasComponent<ApplyInitialHealthModifierComponent>(characterEntity))
                    {
                        healthComponent.CurrentValue = healthComponent.MaxValue;
                        PostUpdateCommands.RemoveComponent<ApplyInitialHealthModifierComponent>(characterEntity);
                    }
                }

                properValue = (magicStatComponent.BaseValue
                                  + magicStatComponent.PerPointValue
                                  * (magicStatComponent.PointsSpent - 1)) + magicModifier;
                if (magicStatComponent.CurrentValue != properValue)
                    magicStatComponent.CurrentValue = properValue;
            });
        }
    }
}
