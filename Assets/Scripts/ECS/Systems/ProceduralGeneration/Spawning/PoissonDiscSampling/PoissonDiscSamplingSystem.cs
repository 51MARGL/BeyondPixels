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
                var requestCells = this.GetCellsByRequestID(poissonDiscSamplingComponent.RequestID, gridSize);
                var validCellList = this.GetValidCells(requestCells);
                if (validCellList.Length == 0)
                    return;

                var finalSamplesList = new NativeList<SampleComponent>(Allocator.Temp);
                var currentSamplesList = new NativeList<SampleComponent>(Allocator.Temp);
                var random = new Random((uint)this.RandomSeed + 1);
                var maxRadius = poissonDiscSamplingComponent.Radius;
                var requestRadiuses = new NativeList<int>(Allocator.Temp);

                if (poissonDiscSamplingComponent.RadiusFromArray == 1)
                    for (var i = 0; i < this.Radiuses.Length; i++)
                        if (this.Radiuses[i].RequestID == poissonDiscSamplingComponent.RequestID)
                        {
                            requestRadiuses.Add(this.Radiuses[i].Radius);
                            if (maxRadius < this.Radiuses[i].Radius)
                                maxRadius = this.Radiuses[i].Radius;
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
                this.RemoveFromValid(randomSample, validCellList);
                var cell = requestCells[randomSample.Position.y * gridSize.x + randomSample.Position.x];
                cell.SampleIndex = finalSamplesList.Length;
                requestCells[randomSample.Position.y * gridSize.x + randomSample.Position.x] = cell;

                while (validCellList.Length > 0)
                {
                    if (currentSamplesList.Length == 0)
                    {
                        randomIndex = random.NextInt(0, validCellList.Length);
                        randomCell = validCellList[randomIndex];
                        var candidate = new SampleComponent
                        {
                            Radius = maxRadius,
                            RequestID = poissonDiscSamplingComponent.RequestID,
                            Position = randomCell.Position
                        };

                        currentSamplesList.Add(candidate);
                        if (this.IsValid(candidate, gridSize, finalSamplesList, requestCells, maxRadius))
                            this.AddToFinal(gridSize, ref requestCells, validCellList, ref finalSamplesList, candidate);
                        else
                            this.RemoveFromValid(candidate, validCellList);
                    }
                    else
                    {
                        var candidateFound = false;

                        randomIndex = random.NextInt(0, currentSamplesList.Length);
                        randomSample = currentSamplesList[randomIndex];

                        for (var i = 0; i < poissonDiscSamplingComponent.SamplesLimit; i++)
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

                            if (this.IsValid(candidate, gridSize, finalSamplesList, requestCells, maxRadius))
                            {
                                this.AddToFinal(gridSize, ref requestCells, validCellList, ref finalSamplesList, candidate);
                                candidateFound = true;
                                break;
                            }
                        }
                        if (!candidateFound)
                        {
                            currentSamplesList.RemoveAtSwapBack(randomIndex);
                            this.RemoveFromValid(randomSample, validCellList);
                        }
                    }
                }

                for (var i = 0; i < finalSamplesList.Length; i++)
                {
                    var sampleEntity = this.CommandBuffer.CreateEntity(index);
                    this.CommandBuffer.AddComponent(index, sampleEntity, finalSamplesList[i]);
                }

                this.CommandBuffer.DestroyEntity(index, entity);
            }

            private void AddToFinal(int2 gridSize, ref NativeArray<PoissonCellComponent> requestCells, NativeList<PoissonCellComponent> validCellList, ref NativeList<SampleComponent> finalSamplesList, SampleComponent candidate)
            {
                finalSamplesList.Add(candidate);
                this.RemoveFromValid(candidate, validCellList);
                var cell = requestCells[candidate.Position.y * gridSize.x + candidate.Position.x];
                cell.SampleIndex = finalSamplesList.Length;
                requestCells[candidate.Position.y * gridSize.x + candidate.Position.x] = cell;
            }

            private NativeArray<PoissonCellComponent> GetCellsByRequestID(int id, int2 gridSize)
            {
                var cells = new NativeArray<PoissonCellComponent>(gridSize.y * gridSize.x, Allocator.Temp);
                var offset = this.GetFirstCellIndexByRequest(id);
                if (offset == -1)
                    return cells;

                for (var y = 0; y < gridSize.y; y++)
                    for (var x = 0; x < gridSize.x; x++)
                        if (this.Cells[offset + (gridSize.x * y + x)].RequestID == id)
                            cells[gridSize.x * y + x] = this.Cells[offset + (gridSize.x * y + x)];

                return cells;
            }

            private int GetFirstCellIndexByRequest(int id)
            {
                for (var i = 0; i < this.Cells.Length; i++)
                    if (this.Cells[i].RequestID == id)
                        return i;
                return -1;
            }

            private NativeList<PoissonCellComponent> GetValidCells(NativeArray<PoissonCellComponent> cells)
            {
                var validCellList = new NativeList<PoissonCellComponent>(Allocator.Temp);
                for (var i = 0; i < cells.Length; i++)
                    if (cells[i].SampleIndex == -1)
                        validCellList.Add(cells[i]);

                return validCellList;
            }

            private void RemoveFromValid(SampleComponent sample, NativeList<PoissonCellComponent> validList)
            {
                for (var i = 0; i < validList.Length; i++)
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

                    for (var y = startY; y < endY; y++)
                        for (var x = startX; x < endX; x++)
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
                this.CommandBuffer.DestroyEntity(index, entity);
            }
        }
        private struct CleanUpRadiusesJob : IJobProcessComponentDataWithEntity<PoissonRadiusComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity, int index, [ReadOnly] ref PoissonRadiusComponent poissonRadiusComponent)
            {
                this.CommandBuffer.DestroyEntity(index, entity);
            }
        }

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;
        private ComponentGroup _cellGroup;
        private ComponentGroup _radiusGroup;

        protected override void OnCreateManager()
        {
            this._endFrameBarrier = World.Active.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
            this._cellGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(PoissonCellComponent)
                }
            });
            this._radiusGroup = this.GetComponentGroup(new EntityArchetypeQuery
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
            var barrier = this._endFrameBarrier.CreateCommandBuffer();

            var generateSamplesJobHandle = new GenerateSamplesJob
            {
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                Cells = this._cellGroup.ToComponentDataArray<PoissonCellComponent>(Allocator.TempJob),
                Radiuses = this._radiusGroup.ToComponentDataArray<PoissonRadiusComponent>(Allocator.TempJob),
                RandomSeed = random.NextInt()
            }.Schedule(this, inputDeps);

            var cleanUpCellsJobHandle = new CleanUpCellsJob
            {
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, generateSamplesJobHandle);

            var cleanUpRadiusesJobHandle = new CleanUpRadiusesJob
            {
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, cleanUpCellsJobHandle);
            this._endFrameBarrier.AddJobHandleForProducer(cleanUpRadiusesJobHandle);
            return cleanUpRadiusesJobHandle;
        }
    }
}
