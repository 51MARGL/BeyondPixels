using BeyondPixels.ECS.Components.Characters.Stats;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.Stats
{
    public class DefenceStatSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(AdjustStatsComponent))]
        private struct DefenceStatJob : IJobForEach<DefenceStatComponent>
        {
            public void Execute(ref DefenceStatComponent defenceStatComponent)
            {
                var properValue = defenceStatComponent.BaseValue
                                  + defenceStatComponent.PerPointValue
                                  * (defenceStatComponent.PointsSpent - 1);
                if (defenceStatComponent.CurrentValue != properValue)
                    defenceStatComponent.CurrentValue = properValue;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new DefenceStatJob().Schedule(this, inputDeps);
        }
    }
}
