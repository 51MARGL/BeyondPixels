using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Stats;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.Stats
{
    public class HealthStatSystem : JobComponentSystem
    {
        [RequireComponentTag(typeof(LevelUpComponent))]
        private struct HealthStatJob : IJobProcessComponentData<HealthComponent, HealthStatComponent, LevelComponent>
        {
            public void Execute(ref HealthComponent healthComponent,
                                ref HealthStatComponent healthStatComponent,
                                [ReadOnly] ref LevelComponent levelComponent)
            {
                var properValue = healthStatComponent.BaseValue
                                  + healthStatComponent.PerLevelValue
                                  * (levelComponent.CurrentLevel - 1);

                if (healthStatComponent.CurrentValue != properValue)
                    healthStatComponent.CurrentValue = properValue;

                if (healthStatComponent.CurrentValue != healthComponent.MaxValue)
                {
                    healthComponent.MaxValue = healthStatComponent.CurrentValue;
                    healthComponent.CurrentValue = healthStatComponent.CurrentValue;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new HealthStatJob().Schedule(this, inputDeps);
        }
    }
}
