using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Stats;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.Stats
{
    public class MagicStatSystem : JobComponentSystem
    {
        [RequireComponentTag(typeof(LevelUpComponent))]
        private struct DefenceStatJob : IJobProcessComponentData<MagicStatComponent, LevelComponent>
        {
            public void Execute(ref MagicStatComponent magicStatComponent,
                                [ReadOnly] ref LevelComponent levelComponent)
            {
                var properValue = magicStatComponent.BaseValue
                                  + magicStatComponent.PerLevelValue
                                  * (levelComponent.CurrentLevel - 1);
                if (magicStatComponent.CurrentValue != properValue)
                    magicStatComponent.CurrentValue = properValue;

                if (magicStatComponent.CurrentValue != magicStatComponent.PerLevelValue * levelComponent.CurrentLevel)
                    magicStatComponent.CurrentValue = magicStatComponent.PerLevelValue * levelComponent.CurrentLevel;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new DefenceStatJob().Schedule(this, inputDeps);
        }
    }
}
