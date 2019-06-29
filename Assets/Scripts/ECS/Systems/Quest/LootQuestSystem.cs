using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.ECS.Components.Quest;
using BeyondPixels.ECS.Systems.Items;
using Unity.Entities;

namespace BeyondPixels.ECS.Systems.Quest
{
    [UpdateBefore(typeof(DropLootSystem))]
    public class LootQuestSystem : ComponentSystem
    {
        private EntityQuery _questGroup;
        private EntityQuery _lootGroup;

        protected override void OnCreate()
        {
            this._questGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(QuestComponent), typeof(LootQuestComponent)
                },
                None = new ComponentType[]
                {
                    typeof(QuestDoneComponent)
                }
            });
            this._lootGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(ChestComponent), typeof(DropLootComponent)
                }
            });
        }
        protected override void OnUpdate()
        {
            this.Entities.With(this._lootGroup).ForEach((Entity entity) =>
            {
                this.Entities.With(this._questGroup).ForEach((Entity questEntity, ref QuestComponent questComponent) =>
                {
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