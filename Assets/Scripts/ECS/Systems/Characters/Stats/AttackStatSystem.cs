﻿using BeyondPixels.ECS.Components.Characters.Stats;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.Stats
{
    public class AttackStatSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(AdjustStatsComponent))]
        private struct AttackStatJob : IJobForEach<AttackStatComponent>
        {
            public void Execute(ref AttackStatComponent attackStatComponent)
            {
                var properValue = attackStatComponent.BaseValue
                                  + attackStatComponent.PerPointValue
                                  * (attackStatComponent.PointsSpent - 1);
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
