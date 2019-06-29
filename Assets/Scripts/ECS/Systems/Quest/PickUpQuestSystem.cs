using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.Quest;
using BeyondPixels.ECS.Systems.Items;
using Unity.Entities;

namespace BeyondPixels.ECS.Systems.Quest
{
    [UpdateBefore(typeof(PickUpSystem))]
    public class PickUpQuestSystem : ComponentSystem
    {
        private EntityQuery _questGroup;
        private EntityQuery _pickGroup;

        protected override void OnCreate()
        {
            this._questGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(QuestComponent), typeof(PickUpQuestComponent)
                },
                None = new ComponentType[]
                {
                    typeof(QuestDoneComponent)
                }
            });
            this._pickGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(PickUpComponent)
                }
            });
        }
        protected override void OnUpdate()
        {
            this.Entities.With(this._pickGroup).ForEach((Entity entity, ref PickUpComponent pickUpComponent) =>
            {
                var itemComponent = this.EntityManager.GetComponentData<ItemComponent>(pickUpComponent.ItemEntity);
                this.Entities.With(this._questGroup).ForEach((Entity questEntity,
                    ref QuestComponent questComponent,
                    ref PickUpQuestComponent pickUpQuestComponent) =>
                {
                    var questItemType = pickUpQuestComponent.ItemType;

                    var item = ItemsManagerComponent.Instance.ItemsStoreComponent.Items[itemComponent.StoreIndex];
                    if (item.ItemType == questItemType)
                        questComponent.CurrentProgress++;

                    if (questComponent.CurrentProgress >= questComponent.ProgressTarget)
                    {
                        questComponent.CurrentProgress = questComponent.ProgressTarget;
                        this.PostUpdateCommands.AddComponent(questEntity, new QuestDoneComponent());
                    }
                });
            });
        }
    }
}