using BeyondPixels.ECS.Components.Quest;
using BeyondPixels.ECS.Components.Scenes;
using BeyondPixels.ECS.Systems.Items;

using Unity.Entities;

namespace BeyondPixels.ECS.Systems.Quest
{
    [UpdateBefore(typeof(DropLootSystem))]
    public class InvestigateQuestSystem : ComponentSystem
    {
        private EntityQuery _questGroup;
        private EntityQuery _invGroup;

        protected override void OnCreate()
        {
            this._questGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(QuestComponent), typeof(InvestigateQuestComponent)
                },
                None = new ComponentType[]
                {
                    typeof(QuestDoneComponent)
                }
            });
            this._invGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(InvestigatedComponent)
                }
            });
        }
        protected override void OnUpdate()
        {
            this.Entities.With(this._invGroup).ForEach((Entity entity) =>
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

                this.PostUpdateCommands.DestroyEntity(entity);
            });
        }
    }
}