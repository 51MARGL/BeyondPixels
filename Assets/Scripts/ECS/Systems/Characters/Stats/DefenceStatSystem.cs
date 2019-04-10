using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Stats;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.Stats
{
    public class DefenceStatSystem : JobComponentSystem
    {
        [RequireComponentTag(typeof(LevelUpComponent))]
        private struct DefenceStatJob : IJobProcessComponentData<DefenceStatComponent, LevelComponent>
        {
            public void Execute(ref DefenceStatComponent defenceStatComponent,
                                [ReadOnly] ref LevelComponent levelComponent)
            {
                var properValue = defenceStatComponent.BaseValue
                                  + defenceStatComponent.PerLevelValue
                                  * (levelComponent.CurrentLevel - 1);
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
