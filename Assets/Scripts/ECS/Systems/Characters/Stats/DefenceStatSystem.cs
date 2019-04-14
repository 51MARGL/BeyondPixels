using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Stats;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.Stats
{
    public class DefenceStatSystem : JobComponentSystem
    {
        [RequireComponentTag(typeof(AdjustStatsComponent))]
        private struct DefenceStatJob : IJobProcessComponentData<DefenceStatComponent>
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
