using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.Player
{
    public class AttackSystem : JobComponentSystem
    {
        [ExcludeComponent(typeof(AttackComponent))]
        private struct AttackInitialJob :
            IJobProcessComponentDataWithEntity<InputComponent, CharacterComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity,
                                int index,
                                [ReadOnly] ref InputComponent input,
                                [ReadOnly] ref CharacterComponent evadeComponent)
            {
                if (input.AttackButtonPressed == 1)
                    CommandBuffer.AddComponent(index, entity,
                        new AttackComponent
                        {
                            CurrentComboIndex = 0
                        });
            }
        }

        private struct AttackComboJob :
            IJobProcessComponentDataWithEntity<AttackComponent, InputComponent, CharacterComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity,
                                int index,
                                ref AttackComponent attackComponent,
                                [ReadOnly] ref InputComponent input,
                                [ReadOnly] ref CharacterComponent evadeComponent)
            {
                // hard coded number of attacks
                if (input.AttackButtonPressed == 1)
                    attackComponent.CurrentComboIndex = (attackComponent.CurrentComboIndex + 1) % 2;
            }
        }

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;

        protected override void OnCreateManager()
        {
            _endFrameBarrier = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var AttackInitialJobHandle = new AttackInitialJob
            {
                CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, inputDeps);
            _endFrameBarrier.AddJobHandleForProducer(AttackInitialJobHandle);

            var AttackComboJobHandle = new AttackComboJob
            {
                CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, AttackInitialJobHandle);
            _endFrameBarrier.AddJobHandleForProducer(AttackComboJobHandle);
            return AttackComboJobHandle;
        }
    }
}

