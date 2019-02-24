using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Player
{
    public class SpellCastingSystem : JobComponentSystem
    {
        [DisableAutoCreation]
        public class SpellCastBarrier : BarrierSystem { }

        [RequireComponentTag(typeof(SpellBookComponent))]
        [RequireSubtractiveComponent(typeof(SpellCastingComponent), typeof(AttackComponent))]
        private struct SpellCastJob : IJobProcessComponentDataWithEntity<InputComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            public float CurrentTime;

            public void Execute(Entity entity,
                                int index,
                                [ReadOnly] ref InputComponent inputComponent)
            {
                if (inputComponent.ActionButtonPressed > 0)
                {
                    CommandBuffer.AddComponent(index, entity, new SpellCastingComponent
                    {
                        SpellIndex = inputComponent.ActionButtonPressed - 1,
                        StartedAt = CurrentTime
                    });
                    inputComponent.ActionButtonPressed = 0;
                }
            }
        }
        [Inject]
        private SpellCastBarrier _spellCastBarrier;

        protected override void OnCreateManager()
        {
            _spellCastBarrier = World.Active.GetOrCreateManager<SpellCastBarrier>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var handle = new SpellCastJob
            {
                CommandBuffer = _spellCastBarrier.CreateCommandBuffer().ToConcurrent(),
                CurrentTime = Time.time
            }.Schedule(this, inputDeps);
            _spellCastBarrier.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}
