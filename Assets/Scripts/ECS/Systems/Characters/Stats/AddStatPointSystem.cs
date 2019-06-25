using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Stats;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.Stats
{
    public class AddStatPointSystem : JobComponentSystem
    {
        private struct AddStatPointJob : IJobForEachWithEntity<AddStatPointComponent, LevelComponent, HealthStatComponent, AttackStatComponent, DefenceStatComponent, MagicStatComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity, int index,
                                [ReadOnly] ref AddStatPointComponent addStatComponent,
                                ref LevelComponent levelComponent,
                                ref HealthStatComponent healthStatComponent,
                                ref AttackStatComponent attackStatComponent,
                                ref DefenceStatComponent defenceStatComponent,
                                ref MagicStatComponent magicStatComponent)
            {
                if (levelComponent.SkillPoints > 0)
                {
                    switch (addStatComponent.StatTarget)
                    {
                        case StatTarget.HealthStat:
                            healthStatComponent.PointsSpent++;
                            break;
                        case StatTarget.AttackStat:
                            attackStatComponent.PointsSpent++;
                            break;
                        case StatTarget.DefenceStat:
                            defenceStatComponent.PointsSpent++;
                            break;
                        case StatTarget.MagicStat:
                            magicStatComponent.PointsSpent++;
                            break;
                    }
                    levelComponent.SkillPoints--;
                    this.CommandBuffer.AddComponent(index, entity, new AdjustStatsComponent());
                }
                this.CommandBuffer.RemoveComponent<AddStatPointComponent>(index, entity);
            }
        }

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;

        protected override void OnCreateManager()
        {
            this._endFrameBarrier = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var handle = new AddStatPointJob
            {
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, inputDeps);
            this._endFrameBarrier.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}
