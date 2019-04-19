using BeyondPixels.ColliderEvents;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Systems.Characters.Stats;
using Unity.Entities;

namespace BeyondPixels.ECS.Systems.Items
{
    [UpdateBefore(typeof(ModifyDamageWithStatsSystem))]
    public class ModifyDamageWithItemsSystem : ComponentSystem
    {
        private ComponentGroup _group;
        private ComponentGroup _gearGroup;

        protected override void OnCreateManager()
        {
            this._group = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(CollisionInfo), typeof(FinalDamageComponent)
                }
            });
            this._gearGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(ItemComponent), typeof(EquipedComponent), typeof(PickedUpComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._group).ForEach((ref CollisionInfo collisionInfo, ref FinalDamageComponent finalDamageComponent) =>
            {
                var sender = collisionInfo.Sender;
                var target = collisionInfo.Target;
                switch (finalDamageComponent.DamageType)
                {
                    case DamageType.Weapon:
                        var attackModifier = 0f;
                        var defenceModifier = 0f;
                        this.Entities.With(this._gearGroup).ForEach((ref ItemComponent itemComponent, ref PickedUpComponent pickedUpComponent) =>
                        {
                            if (pickedUpComponent.Owner == sender)
                            {
                                var item = ItemsManagerComponent.Instance.ItemsStoreComponent.Items[itemComponent.StoreIndex];
                                if (item.ItemType == ItemType.Gear && item.ModifierType == ModifierType.WeaponDamage)
                                    attackModifier += item.ModifierValue * itemComponent.Level;
                            }
                            else if (pickedUpComponent.Owner == target)
                            {
                                var item = ItemsManagerComponent.Instance.ItemsStoreComponent.Items[itemComponent.StoreIndex];
                                if (item.ItemType == ItemType.Gear && item.ModifierType == ModifierType.Armor)
                                    defenceModifier += item.ModifierValue * itemComponent.Level;
                            }
                        });
                        attackModifier *= (finalDamageComponent.DamageAmount / 100f);
                        defenceModifier *= (finalDamageComponent.DamageAmount / 100f);
                        finalDamageComponent.DamageAmount += attackModifier - defenceModifier;
                        break;
                    case DamageType.Magic:
                        var casterMagicModifier = 0f;
                        this.Entities.With(this._gearGroup).ForEach((ref ItemComponent itemComponent, ref PickedUpComponent pickedUpComponent) =>
                        {
                            if (pickedUpComponent.Owner == sender)
                            {
                                var item = ItemsManagerComponent.Instance.ItemsStoreComponent.Items[itemComponent.StoreIndex];
                                if (item.ItemType == ItemType.Gear && item.ModifierType == ModifierType.MagicDamage)
                                    casterMagicModifier += item.ModifierValue * itemComponent.Level;
                            }
                        });
                        casterMagicModifier *= (finalDamageComponent.DamageAmount / 100f);
                        finalDamageComponent.DamageAmount += casterMagicModifier;
                        break;
                }
            });
        }
    }
}
