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
            IJobForEachWithEntity<InputComponent, CharacterComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity,
                                int index,
                                [ReadOnly] ref InputComponent input,
                                [ReadOnly] ref CharacterComponent evadeComponent)
            {
                if (input.AttackButtonPressed == 1)
                    this.CommandBuffer.AddComponent(index, entity,
                        new AttackComponent
                        {
                            CurrentComboIndex = 0
                        });
            }
        }

        private struct AttackComboJob :
            IJobForEachWithEntity<AttackComponent, InputComponent, CharacterComponent>
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
            this._endFrameBarrier = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var AttackInitialJobHandle = new AttackInitialJob
            {
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, inputDeps);
            this._endFrameBarrier.AddJobHandleForProducer(AttackInitialJobHandle);

            var AttackComboJobHandle = new AttackComboJob
            {
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, AttackInitialJobHandle);
            this._endFrameBarrier.AddJobHandleForProducer(AttackComboJobHandle);
            return AttackComboJobHandle;
        }
    }
}

