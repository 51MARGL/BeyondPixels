using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Stats;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Systems.Characters.Stats
{
    public class HealthStatSystem : JobComponentSystem
    {
        [RequireComponentTag(typeof(AdjustStatsComponent))]
        private struct HealthStatJob : IJobProcessComponentData<HealthComponent, HealthStatComponent>
        {
            public void Execute(ref HealthComponent healthComponent,
                                ref HealthStatComponent healthStatComponent)
            {
                var properValue = healthStatComponent.BaseValue
                                  + healthStatComponent.PerPointValue
                                  * (healthStatComponent.PointsSpent - 1);

                if (healthStatComponent.CurrentValue != properValue)
                {
                    healthStatComponent.CurrentValue = properValue;
                    healthComponent.MaxValue = healthComponent.BaseValue 
                        + (healthComponent.BaseValue / 100f * properValue * math.log2(properValue));
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new HealthStatJob().Schedule(this, inputDeps);
        }
    }
}
