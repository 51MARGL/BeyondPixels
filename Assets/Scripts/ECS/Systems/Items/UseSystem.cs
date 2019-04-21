using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Items;
using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Systems.Items
{
    public class UseSystem : ComponentSystem
    {
        private ComponentGroup _itemGroup;

        protected override void OnCreateManager()
        {
            this._itemGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(ItemComponent), typeof(PickedUpComponent), typeof(UseComponent)
                }
            });
        }
        protected override void OnUpdate()
        {
            this.Entities.With(this._itemGroup).ForEach((Entity entity, ref PickedUpComponent pickedUpComponent, ref ItemComponent itemComponent) =>
            {
                var item = ItemsManagerComponent.Instance.ItemsStoreComponent.Items[itemComponent.StoreIndex];
                var ownerEntity = pickedUpComponent.Owner;
                switch (item.ItemType)
                {
                    case ItemType.Food:
                    case ItemType.Potion:
                        var value = item.ModifierValue;
                        var healthComponent = this.EntityManager.GetComponentData<HealthComponent>(ownerEntity);
                        healthComponent.CurrentValue += healthComponent.MaxValue / 100 * value;
                        healthComponent.CurrentValue = math.min(healthComponent.CurrentValue, healthComponent.MaxValue);
                        PostUpdateCommands.SetComponent(ownerEntity, healthComponent);
                        PostUpdateCommands.DestroyEntity(entity);
                        break;
                    case ItemType.Gear:
                        if (this.EntityManager.HasComponent<EquipedComponent>(entity))
                            this.PostUpdateCommands.RemoveComponent<EquipedComponent>(entity);
                        else
                        {
                            this.Entities.WithAll<ItemComponent, PickedUpComponent, EquipedComponent>().ForEach((Entity equipedEntity, ref PickedUpComponent pickedUpEquipedComponent, ref ItemComponent equipedItemComponent) => {
                                if (ownerEntity == pickedUpEquipedComponent.Owner)
                                {
                                    var equipedItem = ItemsManagerComponent.Instance.ItemsStoreComponent.Items[equipedItemComponent.StoreIndex];
                                    if (equipedItem.ItemType == ItemType.Gear && equipedItem.GearType == item.GearType)
                                        PostUpdateCommands.RemoveComponent<EquipedComponent>(equipedEntity);
                                }
                            });
                            this.PostUpdateCommands.AddComponent(entity, new EquipedComponent());
                        }
                        break;
                }

                this.PostUpdateCommands.RemoveComponent<UseComponent>(entity);
            });
        }
    }
}
