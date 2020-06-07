using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.UI.Buttons;
using BeyondPixels.UI.ECS.Components;

using Unity.Entities;

using UnityEngine;

using static BeyondPixels.UI.ECS.Components.PlayerInfoMenuUIComponent;

namespace BeyondPixels.UI.ECS.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class LootBagMenuUISystem : ItemSorterSystem
    {
        private EntityQuery _bagGroup;
        private EntityQuery _playerGroup;
        private EntityQuery _lootButtonEventsGroup;

        protected override void OnCreate()
        {
            base.OnCreate();

            this._bagGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(LootBagComponent), typeof(OpenLootBagComponent)
                }
            });
            this._playerGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(PlayerComponent)
                },
                None = new ComponentType[] {
                    typeof(InCutsceneComponent),
                }
            });
            this._lootButtonEventsGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(LootItemButtonPressedComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._bagGroup).ForEach((Entity bagEntity,
                ref OpenLootBagComponent openLootBagComponent) =>
            {
                var lootMenuComponent = UIManager.Instance.LootBagMenuUIComponent;

                if (openLootBagComponent.IsOpened == 0
                    && !lootMenuComponent.IsVisible)
                {
                    UIManager.Instance.QuestMenu.Hide();
                    UIManager.Instance.PlayerInfoMenuUIComponent.Hide();
                    lootMenuComponent.Show();
                    openLootBagComponent.IsOpened = 1;
                }
                else if (openLootBagComponent.IsOpened == 1
                        && !lootMenuComponent.IsVisible)
                {
                    this.PostUpdateCommands.RemoveComponent<OpenLootBagComponent>(bagEntity);
                    return;
                }

                if (lootMenuComponent.IsVisible)
                {
                    var inventoryGroup = lootMenuComponent.LootGroup;
                    var itemCount = this._itemsGroup.CalculateEntityCount();
                    this.SetUpInventoryItems(inventoryGroup, itemCount, bagEntity);
                }
            });

            this.Entities.With(this._lootButtonEventsGroup).ForEach((Entity eventEntity, ref LootItemButtonPressedComponent eventComponent) =>
            {
                this.LastItemsCount = -1;

                var eComponent = eventComponent;
                this.Entities.With(this._playerGroup).ForEach((Entity playerEntity) =>
                {
                    var pickupEventEntity = this.PostUpdateCommands.CreateEntity();
                    this.PostUpdateCommands.RemoveComponent<PickedUpComponent>(eComponent.ItemEntity);
                    this.PostUpdateCommands.AddComponent(pickupEventEntity, new PickUpComponent
                    {
                        ItemEntity = eComponent.ItemEntity,
                        Owner = playerEntity
                    });
                });

                this.PostUpdateCommands.DestroyEntity(eventEntity);
            });
        }

        protected override int SetUpInventoryItems(InventoryGroupWrapper inventoryGroup, int itemCount, Entity owner)
        {
            var resItemCount = base.SetUpInventoryItems(inventoryGroup, itemCount, owner);
            if (resItemCount == 0)
            {
                UIManager.Instance.LootBagMenuUIComponent.Hide();

                this.PostUpdateCommands.AddComponent(owner, new DestroyComponent());
            }
            return resItemCount;
        }

        protected override ItemButton AddInventoryButton(ItemHashValue hashValue, InventoryGroupWrapper inventoryGroup)
        {
            var button = GameObject.Instantiate(inventoryGroup.ItemButtonPrefab, inventoryGroup.Grid.transform).GetComponent<LootItemButton>();
            var item = ItemsManagerComponent.Instance.ItemsStoreComponent.Items[hashValue.ItemComponent.StoreIndex];
            button.ItemEntity = hashValue.Entity;
            button.ItemIcon.enabled = true;
            button.ItemIcon.sprite = item.Icons[hashValue.ItemComponent.IconIndex];
            return button;
        }
    }
}
