using BeyondPixels.Components.Characters.Common;
using BeyondPixels.Components.Characters.Player;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace BeyondPixels.Systems.Characters.Player
{
    public class SpellCastingSystem : JobComponentSystem
    {       
        [RequireComponentTag(typeof(SpellBookComponent))]
        [RequireSubtractiveComponent(typeof(SpellCastingComponent))]
        private struct SpellCastJob : IJobProcessComponentDataWithEntity<InputComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            public float CurrentTime;

            public void Execute(Entity entity,
                                int index,
                                [ReadOnly] ref InputComponent inputComponent)
            {
                if (inputComponent.ActionButtonPressed > 0)
                    CommandBuffer.AddComponent(entity, new SpellCastingComponent
                    {
                        SpellIndex = inputComponent.ActionButtonPressed - 1,
                        StartedAt = CurrentTime
                    });
            }
        }
        [Inject]
        private SpellCastBarrier _barrier;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new SpellCastJob
            {
                CommandBuffer = _barrier.CreateCommandBuffer(),
                CurrentTime = Time.time
            }.Schedule(this, inputDeps);
        }

        public class SpellCastBarrier : BarrierSystem { }
    }
}
