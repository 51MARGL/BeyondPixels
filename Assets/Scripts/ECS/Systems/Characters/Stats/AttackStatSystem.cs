using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Stats;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.Stats
{    
    public class AttackStatSystem : JobComponentSystem
    {
        [RequireComponentTag(typeof(LevelUpComponent))]
        private struct AttackStatJob : IJobProcessComponentData<AttackStatComponent, LevelComponent>
        {
            public void Execute(ref AttackStatComponent attackStatComponent,
                                [ReadOnly] ref LevelComponent levelComponent)
            {
                var properValue = attackStatComponent.BaseValue
                                  + attackStatComponent.PerLevelValue
                                  * (levelComponent.CurrentLevel - 1);
                if (attackStatComponent.CurrentValue != properValue)
                    attackStatComponent.CurrentValue = properValue;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new AttackStatJob().Schedule(this, inputDeps);
        }
    }
}
