using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Quest;
using BeyondPixels.ECS.Systems.Characters.Stats;

using Unity.Entities;

namespace BeyondPixels.ECS.Systems.Quest
{
    [UpdateBefore(typeof(AddStatPointSystem))]
    public class SpendSkillPointQuestSystem : ComponentSystem
    {
        private EntityQuery _questGroup;
        private EntityQuery _pointGroup;

        protected override void OnCreate()
        {
            this._questGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(QuestComponent), typeof(SpendSkillPointQuestComponent)
                },
                None = new ComponentType[]
                {
                    typeof(QuestDoneComponent)
                }
            });
            this._pointGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(AddStatPointComponent)
                }
            });
        }
        protected override void OnUpdate()
        {
            this.Entities.With(this._pointGroup).ForEach((Entity entity) =>
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