using System.Linq;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Spells;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Spells
{
    public class SpellCoolDownSystem : JobComponentSystem
    {
        private struct SpellCoolDownJob : IJobProcessComponentDataWithEntity<ActiveSpellComponent, CoolDownComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            [ReadOnly]
            public float DeltaTime;

            public void Execute(Entity entity, int index, 
                [ReadOnly] ref ActiveSpellComponent activeSpellComponent, 
                ref CoolDownComponent coolDownComponent)
            {
                coolDownComponent.CoolDownTime -= DeltaTime;
                if (coolDownComponent.CoolDownTime < 0)
                    CommandBuffer.RemoveComponent<CoolDownComponent>(index, entity);
            }
        }
        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;

        protected override void OnCreateManager()
        {
            _endFrameBarrier = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var coolDownJobHandle = new SpellCoolDownJob
            {
                CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                DeltaTime = Time.deltaTime
            }.Schedule(this, inputDeps);
            _endFrameBarrier.AddJobHandleForProducer(coolDownJobHandle);
            return coolDownJobHandle;
        }
    }
}
