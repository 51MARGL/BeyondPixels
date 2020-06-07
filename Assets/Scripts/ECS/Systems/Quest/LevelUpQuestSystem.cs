using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Quest;
using BeyondPixels.ECS.Systems.Characters.Level;

using Unity.Entities;

namespace BeyondPixels.ECS.Systems.Quest
{
    [UpdateBefore(typeof(LevelUpSystem))]
    public class LevelUpQuestSystem : ComponentSystem
    {
        private EntityQuery _questGroup;
        private EntityQuery _levelGroup;

        protected override void OnCreate()
        {
            this._questGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(QuestComponent), typeof(LevelUpQuestComponent)
                },
                None = new ComponentType[]
                {
                    typeof(QuestDoneComponent)
                }
            });
            this._levelGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(LevelUpComponent)
                }
            });
        }
        protected override void OnUpdate()
        {
            this.Entities.With(this._levelGroup).ForEach((Entity entity) =>
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