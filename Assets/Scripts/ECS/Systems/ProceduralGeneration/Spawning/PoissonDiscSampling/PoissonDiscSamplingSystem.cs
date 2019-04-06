using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling;
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
            [DeallocateOnJobCompletion]
            public NativeArray<PoissonRadiusComponent> Radiuses;
            [ReadOnly]
            public int RandomSeed;

            public void Execute(Entity entity, int index, [ReadOnly] ref PoissonDiscSamplingComponent poissonDiscSamplingComponent)
            {
                var gridSize = poissonDiscSamplingComponent.GridSize;
                var requestCells = GetCellsByRequestID(poissonDiscSamplingComponent.RequestID, gridSize);
                var validCellList = GetValidCells(requestCells);
                var finalSamplesList = new NativeList<SampleComponent>(Allocator.Temp);
                var currentSamplesList = new NativeList<SampleComponent>(Allocator.Temp);
                var random = new Random((uint)RandomSeed + 1);
                var maxRadius = poissonDiscSamplingComponent.Radius;
                var requestRadiuses = new NativeList<int>(Allocator.Temp);

                if (poissonDiscSamplingComponent.RadiusFromArray == 1)
                    for (int i = 0; i < Radiuses.Length; i++)
                        if (Radiuses[i].RequestID == poissonDiscSamplingComponent.RequestID)
                        {
                            requestRadiuses.Add(Radiuses[i].Radius);
                            if (maxRadius < Radiuses[i].Radius)
                                maxRadius = Radiuses[i].Radius;
                        }

                var randomIndex = random.NextInt(0, validCellList.Length);
                var randomCell = validCellList[randomIndex];
                var randomSample = new SampleComponent
                {
                    Radius = maxRadius,
                    RequestID = poissonDiscSamplingComponent.RequestID,
                    Position = randomCell.Position
                };
                currentSamplesList.Add(randomSample);
                finalSamplesList.Add(randomSample);
                RemoveFromValid(randomSample, validCellList);
                var cell = requestCells[randomSample.Position.y * gridSize.x + randomSample.Position.x];
                cell.SampleIndex = finalSamplesList.Length;
                requestCells[randomSample.Position.y * gridSize.x + randomSample.Position.x] = cell;

                while (validCellList.Length > 0)
                {
                    if (currentSamplesList.Length == 0)
                    {
                        randomIndex = random.NextInt(0, validCellList.Length);
                        randomCell = validCellList[randomIndex];
                        currentSamplesList.Add(new SampleComponent
                        {
                            Radius = maxRadius,
                            RequestID = poissonDiscSamplingComponent.RequestID,
                            Position = randomCell.Position
                        });
                    }
                    randomIndex = random.NextInt(0, currentSamplesList.Length);
                    randomSample = currentSamplesList[randomIndex];

                    var candidateFound = false;

                    for (int i = 0; i < poissonDiscSamplingComponent.SamplesLimit; i++)
                    {
                        var randomAngle = (float)(random.NextInt() * math.PI * 2);
                        var randomDirection = new float2(math.sin(randomAngle), math.cos(randomAngle));
                        var radius = maxRadius;
                        if (poissonDiscSamplingComponent.RadiusFromArray == 1)
                            radius = requestRadiuses[random.NextInt(0, requestRadiuses.Length)];

                        var candidate = new SampleComponent
                        {
                            Radius = radius,
                            RequestID = poissonDiscSamplingComponent.RequestID,
                            Position =
                            (int2)(randomSample.Position + randomDirection *
                                   random.NextInt(radius, 2 * radius))
                        };

                        if (IsValid(candidate, gridSize, finalSamplesList, requestCells, maxRadius))
                        {
                            finalSamplesList.Add(candidate);
                            RemoveFromValid(candidate, validCellList);
                            cell = requestCells[candidate.Position.y * gridSize.x + candidate.Position.x];
                            cell.SampleIndex = finalSamplesList.Length;
                            requestCells[candidate.Position.y * gridSize.x + candidate.Position.x] = cell;
                            candidateFound = true;
                            break;
                        }
                    }
                    if (!candidateFound)
                    {
                        currentSamplesList.RemoveAtSwapBack(randomIndex);
                        RemoveFromValid(randomSample, validCellList);
                    }
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

            private void RemoveFromValid(SampleComponent sample, NativeList<PoissonCellComponent> validList)
            {
                for (int i = 0; i < validList.Length; i++)
                    if (sample.Position.Equals(validList[i].Position))
                    {
                        validList.RemoveAtSwapBack(i);
                        return;
                    }
            }

            private bool IsValid(SampleComponent candidate, int2 gridSize, NativeList<SampleComponent> samples, NativeArray<PoissonCellComponent> cells, int maxRadius)
            {
                var candidatePosition = candidate.Position;
                var candidateCellIndex = candidatePosition.y * gridSize.x + candidatePosition.x;
                if (candidatePosition.x >= 0 && candidatePosition.x < gridSize.x
                    && candidatePosition.y >= 0 && candidatePosition.y < gridSize.y
                    && cells[candidateCellIndex].SampleIndex == -1)
                {
                    var startX = math.max(0, candidatePosition.x - 2 * maxRadius);
                    var endX = math.min(candidatePosition.x + 2 * maxRadius, gridSize.x);
                    var startY = math.max(0, candidatePosition.y - 2 * maxRadius);
                    var endY = math.min(candidatePosition.y + 2 * maxRadius, gridSize.y);

                    for (int y = startY; y < endY; y++)
                        for (int x = startX; x < endX; x++)
                        {
                            var sampleIndex = cells[y * gridSize.x + x].SampleIndex - 1;
                            if (sampleIndex > -1)
                            {
                                var distance = math.distance(candidatePosition, samples[sampleIndex].Position);
                                if (distance < samples[sampleIndex].Radius || distance < candidate.Radius)
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
        private struct CleanUpRadiusesJob : IJobProcessComponentDataWithEntity<PoissonRadiusComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity, int index, [ReadOnly] ref PoissonRadiusComponent poissonRadiusComponent)
            {
                CommandBuffer.DestroyEntity(index, entity);
            }
        }

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;
        private ComponentGroup _cellGroup;
        private ComponentGroup _radiusGroup;

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
            _radiusGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(PoissonRadiusComponent)
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
                Radiuses = _radiusGroup.ToComponentDataArray<PoissonRadiusComponent>(Allocator.TempJob),
                RandomSeed = random.NextInt()
            }.Schedule(this, inputDeps);

            var cleanUpCellsJobHandle = new CleanUpCellsJob
            {
                CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, generateSamplesJobHandle);

            var cleanUpRadiusesJobHandle = new CleanUpRadiusesJob
            {
                CommandBuffer = _endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, cleanUpCellsJobHandle);
            _endFrameBarrier.AddJobHandleForProducer(cleanUpRadiusesJobHandle);
            return cleanUpRadiusesJobHandle;
        }
    }
}
