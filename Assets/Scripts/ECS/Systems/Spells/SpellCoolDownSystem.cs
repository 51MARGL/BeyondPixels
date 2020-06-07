using BeyondPixels.ECS.Components.Spells;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Spells
{
    public class SpellCoolDownSystem : JobComponentSystem
    {
        private struct SpellCoolDownJob : IJobForEachWithEntity<ActiveSpellComponent, CoolDownComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            [ReadOnly]
            public float DeltaTime;

            public void Execute(Entity entity, int index,
                [ReadOnly] ref ActiveSpellComponent activeSpellComponent,
                ref CoolDownComponent coolDownComponent)
            {
                coolDownComponent.CoolDownTime -= this.DeltaTime;
                if (coolDownComponent.CoolDownTime < 0)
                {
                    this.CommandBuffer.RemoveComponent<CoolDownComponent>(index, entity);
                }
            }
        }
        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;

        protected override void OnCreate()
        {
            this._endFrameBarrier = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var coolDownJobHandle = new SpellCoolDownJob
            {
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                DeltaTime = Time.deltaTime
            }.Schedule(this, inputDeps);
            this._endFrameBarrier.AddJobHandleForProducer(coolDownJobHandle);
            return coolDownJobHandle;
        }
    }
}
