using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Spawning.PoissonDiscSampling
{
    public class PoissonDiscSamplingSystem : JobComponentSystem
    {
        [DisableAutoCreation]
        private class PoissonDiscSamplingBarrier : BarrierSystem { }

        private struct GenerateSamplesJob : IJob
        {
            public EntityCommandBuffer CommandBuffer;
            [ReadOnly]
            public PoissonDiscSamplingComponent PoissonDiscSamplingComponent;
            public ComponentDataArray<CellComponent> Cells;
            [ReadOnly]
            public int RandomSeed;

            public void Execute()
            {
                var gridSize = PoissonDiscSamplingComponent.GridSize;
                var finalSamplesList = new NativeList<SampleComponent>(Allocator.Temp);
                var validCellList = GetValidCells();
                var random = new Random((uint)RandomSeed + 1);

                while (validCellList.Length > 0)
                {
                    var randomCellIndex = random.NextInt(0, validCellList.Length);
                    var randomCell = validCellList[randomCellIndex];
                    var candidateFound = false;

                    for (int i = 0; i < PoissonDiscSamplingComponent.SamplesLimit; i++)
                    {
                        var randomAngle = (float)(random.NextInt() * math.PI * 2);
                        var randomDirection = new float2(math.sin(randomAngle), math.cos(randomAngle));
                        var candidate = new SampleComponent
                        {
                            Radius = PoissonDiscSamplingComponent.Radius,
                            Position =
                                (int2)(randomCell.Position + randomDirection *
                                       random.NextInt(PoissonDiscSamplingComponent.Radius, 2 * PoissonDiscSamplingComponent.Radius))
                        };

                        if (IsValid(candidate, gridSize, finalSamplesList))
                        {
                            finalSamplesList.Add(candidate);
                            var cell = Cells[candidate.Position.y * gridSize.x + candidate.Position.x];
                            cell.SampleIndex = finalSamplesList.Length;
                            Cells[candidate.Position.y * gridSize.x + candidate.Position.x] = cell;
                            candidateFound = true;
                            break;
                        }
                    }
                    if (!candidateFound)
                        validCellList.RemoveAtSwapBack(randomCellIndex);
                }

                for (int i = 0; i < finalSamplesList.Length; i++)
                {
                    var entity = CommandBuffer.CreateEntity();
                    CommandBuffer.AddComponent(entity, finalSamplesList[i]);
                }
            }

            private NativeList<CellComponent> GetValidCells()
            {
                var validCellList = new NativeList<CellComponent>(Allocator.Temp);
                for (int i = 0; i < Cells.Length; i++)
                    if (Cells[i].SampleIndex == -1)
                        validCellList.Add(Cells[i]);
                    
                return validCellList;
            }

            private bool IsValid(SampleComponent candidate, int2 gridSize, NativeList<SampleComponent> samples)
            {
                var candidatePosition = candidate.Position;
                var candidateCellIndex = candidatePosition.y * gridSize.x + candidatePosition.x;
                if (candidatePosition.x >= 0 && candidatePosition.x < gridSize.x
                    && candidatePosition.y >= 0 && candidatePosition.y < gridSize.y
                    && Cells[candidateCellIndex].SampleIndex == -1)
                {
                    var startX = math.max(0, candidatePosition.x - 2 * candidate.Radius);
                    var endX = math.min(candidatePosition.x + 2 * candidate.Radius, gridSize.x);
                    var startY = math.max(0, candidatePosition.y - 2 * candidate.Radius);
                    var endY = math.min(candidatePosition.y + 2 * candidate.Radius, gridSize.y);

                    for (int y = startY; y < endY; y++)
                        for (int x = startX; x < endX; x++)
                        {
                            var sampleIndex = Cells[y * gridSize.x + x].SampleIndex - 1;
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

        private struct CleanUpJob : IJobParallelFor
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            public EntityArray EntityArray;

            public void Execute(int index)
            {
                CommandBuffer.DestroyEntity(index, EntityArray[index]);
            }
        }

        private struct Data
        {
            public readonly int Length;
            public ComponentDataArray<PoissonDiscSamplingComponent> PoissonDiscSamplingComponents;
            public EntityArray EntityArray;
        }
        [Inject]
        private Data _data;

        private struct CellData
        {
            public readonly int Length;
            public ComponentDataArray<CellComponent> CellComponents;
            public EntityArray EntityArray;
        }
        [Inject]
        private CellData _cellData;

        private PoissonDiscSamplingBarrier _poissonDiscSamplingSystemBarrier;

        protected override void OnCreateManager()
        {
            _poissonDiscSamplingSystemBarrier = World.Active.GetOrCreateManager<PoissonDiscSamplingBarrier>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var random = new Random((uint)System.DateTime.Now.ToString("yyyyMMddHHmmssff").GetHashCode());
            var barrier = _poissonDiscSamplingSystemBarrier.CreateCommandBuffer();

            for (int i = 0; i < _data.Length; i++)
            {
                inputDeps = new GenerateSamplesJob
                {
                    CommandBuffer = _poissonDiscSamplingSystemBarrier.CreateCommandBuffer(),
                    PoissonDiscSamplingComponent = _data.PoissonDiscSamplingComponents[i],
                    Cells = _cellData.CellComponents,
                    RandomSeed = random.NextInt()
                }.Schedule(inputDeps);

                inputDeps = new CleanUpJob
                {
                    CommandBuffer = _poissonDiscSamplingSystemBarrier.CreateCommandBuffer().ToConcurrent(),
                    EntityArray = _cellData.EntityArray
                }.Schedule(_cellData.Length, 1, inputDeps);
            }
            var cleanDataJobHandle = new CleanUpJob
            {
                CommandBuffer = _poissonDiscSamplingSystemBarrier.CreateCommandBuffer().ToConcurrent(),
                EntityArray = _data.EntityArray
            }.Schedule(_data.Length, 1, inputDeps);
            _poissonDiscSamplingSystemBarrier.AddJobHandleForProducer(cleanDataJobHandle);
            return cleanDataJobHandle;
        }
    }
}
