﻿using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Spawning.PoissonDiscSampling
{
    public class PoissonDiscSamplingSystem : JobComponentSystem
    {
        private struct GenerateSamplesJob : IJobProcessComponentDataWithEntity<PoissonDiscSamplingComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<PoissonCellComponent> Cells;
            [ReadOnly]
            public int RandomSeed;

            public void Execute(Entity entity, int index, [ReadOnly] ref PoissonDiscSamplingComponent poissonDiscSamplingComponent)
            {
                var gridSize = poissonDiscSamplingComponent.GridSize;
                var finalSamplesList = new NativeList<SampleComponent>(Allocator.Temp);
                var requestCells = GetCellsByRequestID(poissonDiscSamplingComponent.RequestID, gridSize);
                var validCellList = GetValidCells(requestCells);
                var random = new Random((uint)RandomSeed + 1);

                while (validCellList.Length > 0)
                {
                    var randomCellIndex = random.NextInt(0, validCellList.Length);
                    var randomCell = validCellList[randomCellIndex];
                    var candidateFound = false;

                    for (int i = 0; i < poissonDiscSamplingComponent.SamplesLimit; i++)
                    {
                        var randomAngle = (float)(random.NextInt() * math.PI * 2);
                        var randomDirection = new float2(math.sin(randomAngle), math.cos(randomAngle));
                        var candidate = new SampleComponent
                        {
                            Radius = poissonDiscSamplingComponent.Radius,
                            RequestID = poissonDiscSamplingComponent.RequestID,
                            Position =
                                (int2)(randomCell.Position + randomDirection *
                                       random.NextInt(poissonDiscSamplingComponent.Radius, 2 * poissonDiscSamplingComponent.Radius))
                        };

                        if (IsValid(candidate, gridSize, finalSamplesList, requestCells))
                        {
                            finalSamplesList.Add(candidate);
                            var cell = requestCells[candidate.Position.y * gridSize.x + candidate.Position.x];
                            cell.SampleIndex = finalSamplesList.Length;
                            requestCells[candidate.Position.y * gridSize.x + candidate.Position.x] = cell;
                            candidateFound = true;
                            break;
                        }
                    }
                    if (!candidateFound)
                        validCellList.RemoveAtSwapBack(randomCellIndex);
                }

                for (int i = 0; i < finalSamplesList.Length; i++)
                {
                    var sampleEntity = CommandBuffer.CreateEntity(index);
                    CommandBuffer.AddComponent(index, sampleEntity, finalSamplesList[i]);
                }

                CommandBuffer.DestroyEntity(index, entity);
            }

            private NativeArray<PoissonCellComponent> GetCellsByRequestID(int id, int2 gridSize)
            {
                var cells = new NativeArray<PoissonCellComponent>(gridSize.y * gridSize.x, Allocator.Temp);
                var offset = GetFirstCellIndexByRequest(id);
                if (offset == -1)
                    return cells;

                for (int y = 0; y < gridSize.y; y++)
                    for (int x = 0; x < gridSize.x; x++)
                        if (Cells[offset + (gridSize.x * y + x)].RequestID == id)
                            cells[gridSize.x * y + x] = Cells[offset + (gridSize.x * y + x)];

                return cells;
            }

            private int GetFirstCellIndexByRequest(int id)
            {
                for (int i = 0; i < Cells.Length; i++)
                    if (Cells[i].RequestID == id)
                        return i;
                return -1;
            }

            private NativeList<PoissonCellComponent> GetValidCells(NativeArray<PoissonCellComponent> cells)
            {
                var validCellList = new NativeList<PoissonCellComponent>(Allocator.Temp);
                for (int i = 0; i < cells.Length; i++)
                    if (cells[i].SampleIndex == -1)
                        validCellList.Add(cells[i]);

                return validCellList;
            }

            private bool IsValid(SampleComponent candidate, int2 gridSize, NativeList<SampleComponent> samples, NativeArray<PoissonCellComponent> cells)
            {
                var candidatePosition = candidate.Position;
                var candidateCellIndex = candidatePosition.y * gridSize.x + candidatePosition.x;
                if (candidatePosition.x >= 0 && candidatePosition.x < gridSize.x
                    && candidatePosition.y >= 0 && candidatePosition.y < gridSize.y
                    && cells[candidateCellIndex].SampleIndex == -1)
                {
                    var startX = math.max(0, candidatePosition.x - 2 * candidate.Radius);
                    var endX = math.min(candidatePosition.x + 2 * candidate.Radius, gridSize.x);
                    var startY = math.max(0, candidatePosition.y - 2 * candidate.Radius);
                    var endY = math.min(candidatePosition.y + 2 * candidate.Radius, gridSize.y);

                    for (int y = startY; y < endY; y++)
                        for (int x = startX; x < endX; x++)
                        {
                            var sampleIndex = cells[y * gridSize.x + x].SampleIndex - 1;
                            if (sampleIndex > -1)
                            {
                                var distance = math.distance(candidatePosition, samples[sampleIndex].Position);
                                if (distance < samples[sampleIndex].Radius)
                                    return false;
                            }
                        }
                    return true;
                }
                return false;
            }
        }

        private struct CleanUpCellsJob : IJobProcessComponentDataWithEntity<PoissonCellComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity, int index, [ReadOnly] ref PoissonCellComponent poissonCellComponent)
            {
                CommandBuffer.DestroyEntity(index, entity);
            }
        }

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;
        private ComponentGroup _cellGroup;

        protected override void OnCreateManager()
        {
            _endFrameBarrier = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
            _cellGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(PoissonCellComponent)
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var random = new Random((uint)System.DateTime.Now.ToString("yyyyMMddHHmmssff").GetHashCode());
            var barrier = _endFrameBarrier.CreateCommandBuffer();

            var generateSamplesJobHandle = new GenerateSamplesJob
            {
                CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                Cells = _cellGroup.ToComponentDataArray<PoissonCellComponent>(Allocator.TempJob),
                RandomSeed = random.NextInt()
            }.Schedule(this, inputDeps);

            var cleanUpCellsJobHandle = new CleanUpCellsJob
            {
                CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, generateSamplesJobHandle);
            _endFrameBarrier.AddJobHandleForProducer(cleanUpCellsJobHandle);
            return cleanUpCellsJobHandle;
        }
    }
}
