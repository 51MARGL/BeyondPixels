using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Quest;

using Unity.Entities;

namespace BeyondPixels.ECS.Systems.Quest
{
    public class DefeatQuestSystem : ComponentSystem
    {
        private EntityQuery _questGroup;
        private EntityQuery _killGroup;

        protected override void OnCreate()
        {
            this._questGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(QuestComponent), typeof(DefeatQuestComponent)
                },
                None = new ComponentType[]
                {
                    typeof(QuestDoneComponent)
                }
            });
            this._killGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(HealthComponent), typeof(CharacterComponent),
                    typeof(PositionComponent), typeof(KilledComponent)
                }
            });
        }
        protected override void OnUpdate()
        {
            this.Entities.With(this._killGroup).ForEach((Entity entity, ref CharacterComponent characterComponent) =>
            {
                var type = characterComponent.CharacterType;
                this.Entities.With(this._questGroup).ForEach((Entity questEntity, ref QuestComponent questComponent) =>
                {
                    if (type == CharacterType.Enemy)
                    {
                        questComponent.CurrentProgress++;
                    }

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