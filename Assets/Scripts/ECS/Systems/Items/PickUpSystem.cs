﻿using BeyondPixels.ECS.Components.Items;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BeyondPixels.ECS.Systems.Items
{
    public class PickUpSystem : JobComponentSystem
    {
        private struct PickUpJob : IJobForEachWithEntity<PickUpComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity,
                                int index,
                                [ReadOnly]ref PickUpComponent pickUpComponent)
            {
                this.CommandBuffer.AddComponent(index, pickUpComponent.ItemEntity, new PickedUpComponent
                {
                    Owner = pickUpComponent.Owner
                });
                this.CommandBuffer.DestroyEntity(index, entity);
            }
        }

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;

        protected override void OnCreate()
        {
            this._endFrameBarrier = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var handle = new PickUpJob
            {
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, inputDeps);
            this._endFrameBarrier.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}
