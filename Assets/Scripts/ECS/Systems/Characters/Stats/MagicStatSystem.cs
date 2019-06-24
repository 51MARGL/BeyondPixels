using BeyondPixels.ECS.Components.Characters.Stats;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.Stats
{
    public class MagicStatSystem : JobComponentSystem
    {
        //[BurstCompile]
        [RequireComponentTag(typeof(AdjustStatsComponent))]
        private struct DefenceStatJob : IJobProcessComponentData<MagicStatComponent>
        {
            public void Execute(ref MagicStatComponent magicStatComponent)
            {
                var properValue = magicStatComponent.BaseValue
                                  + magicStatComponent.PerPointValue
                                  * (magicStatComponent.PointsSpent - 1);
                if (magicStatComponent.CurrentValue != properValue)
                    magicStatComponent.CurrentValue = properValue;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new DefenceStatJob().Schedule(this, inputDeps);
        }
    }
}
