using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling;
using BeyondPixels.ECS.Components.Scenes;
using BeyondPixels.SceneBootstraps;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Spawning.PoissonDiscSampling
{
    public class ExitSpawningSystem : JobComponentSystem
    {
        private const int SystemRequestID = 3;

        private struct ExitSpawnStartedComponent : IComponentData { }
        private struct InitializeValidationGridJob : IJobForEachWithEntity<FinalBoardComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<FinalTileComponent> Tiles;
            public float2 PlayerPosition;

            public void Execute(Entity boardEntity, int index, ref FinalBoardComponent finalBoardComponent)
            {
                var poissonDiscEntity = this.CommandBuffer.CreateEntity(index);
                var boardSize = finalBoardComponent.Size;
                var radius = 20;
                this.CommandBuffer.AddComponent(index, poissonDiscEntity, new PoissonDiscSamplingComponent
                {
                    GridSize = boardSize,
                    SamplesLimit = 30,
                    RequestID = SystemRequestID,
                    Radius = radius
                });

                var cellList = this.GetCells(boardSize, radius);

                for (var i = 0; i < cellList.Length; i++)
                {
                    var entity = this.CommandBuffer.CreateEntity(index);
                    this.CommandBuffer.AddComponent(index, entity, cellList[i]);
                }
                this.CommandBuffer.AddComponent(index, boardEntity, new ExitSpawnStartedComponent());
            }

            private NativeList<PoissonCellComponent> GetCells(int2 boardSize, int radius)
            {
                var cellList = new NativeList<PoissonCellComponent>(Allocator.Temp);
                for (var y = 0; y < boardSize.y; y++)
                    for (var x = 0; x < boardSize.x; x++)
                    {
                        var validationIndex = this.GetValidationIndex(y * boardSize.x + x, boardSize, radius);
                        cellList.Add(new PoissonCellComponent
                        {
                            SampleIndex = validationIndex,
                            Position = this.Tiles[y * boardSize.x + x].Position,
                            RequestID = SystemRequestID
                        });
                    }

                return cellList;
            }

            private int GetValidationIndex(int tileIndex, int2 boardSize, int radius)
            {
                var tile = this.Tiles[tileIndex];
                if (tile.TileType == TileType.Wall
                    && math.distance(this.PlayerPosition, tile.Position) >= radius
                    && tile.Position.x > 2 && tile.Position.x < boardSize.x - 2
                    && tile.Position.y > 2 && tile.Position.y < boardSize.y - 2
                    && this.Tiles[(tile.Position.y - 1) * boardSize.x + tile.Position.x].TileType == TileType.Floor
                    && this.Tiles[(tile.Position.y - 1) * boardSize.x + tile.Position.x + 1].TileType == TileType.Floor
                    && this.Tiles[(tile.Position.y - 1) * boardSize.x + tile.Position.x - 1].TileType == TileType.Floor
                    && this.Tiles[(tile.Position.y + 1) * boardSize.x + tile.Position.x].TileType == TileType.Wall
                    && this.Tiles[tile.Position.y * boardSize.x + tile.Position.x + 1].TileType == TileType.Wall
                    && this.Tiles[tile.Position.y * boardSize.x + tile.Position.x - 1].TileType == TileType.Wall)
                    return -1;

                return -2;
            }
        }

        private struct TagBoardDoneJob : IJobForEachWithEntity<FinalBoardComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity, int index, [ReadOnly] ref FinalBoardComponent finalBoardComponent)
            {
                this.CommandBuffer.AddComponent(index, entity, new ExitSpawnedComponent());
            }
        }

        private struct CleanSamplesJob : IJobForEachWithEntity<SampleComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity, int index, [ReadOnly] ref SampleComponent sampleComponent)
            {
                if (sampleComponent.RequestID == SystemRequestID)
                    this.CommandBuffer.DestroyEntity(index, entity);
            }
        }

        private EndSimulationEntityCommandBufferSystem _endFrameBarrier;
        private EntityQuery _tilesGroup;
        private EntityQuery _boardSpawnInitGroup;
        private EntityQuery _boardSpawnReadyGroup;
        private EntityQuery _samplesGroup;

        protected override void OnCreate()
        {
            this._endFrameBarrier = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            this._tilesGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(FinalTileComponent)
                }
            });
            this._boardSpawnInitGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(FinalBoardComponent),
                    typeof(PlayerSpawnedComponent)
                },
                None = new ComponentType[]
                {
                    typeof(ExitSpawnStartedComponent)
                }
            });
            this._boardSpawnReadyGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(FinalBoardComponent), typeof(ExitSpawnStartedComponent)
                },
                None = new ComponentType[]
                {
                    typeof(ExitSpawnedComponent)
                }
            });
            this._samplesGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(SampleComponent)
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (this._boardSpawnInitGroup.CalculateEntityCount() > 0)
                return this.SetupValidationGrid(inputDeps);

            if (this._boardSpawnReadyGroup.CalculateEntityCount() > 0)
            {
                if (this._samplesGroup.CalculateEntityCount() > 0)
                {
                    var samplesArray = this._samplesGroup.ToComponentDataArray<SampleComponent>(Allocator.TempJob);
                    var samplesList = new NativeList<SampleComponent>(Allocator.TempJob);
                    for (var i = 0; i < samplesArray.Length; i++)
                        if (samplesArray[i].RequestID == SystemRequestID)
                            samplesList.Add(samplesArray[i]);

                    var tagBoardDoneJobHandle = new TagBoardDoneJob
                    {
                        CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                    }.Schedule(this, inputDeps);
                    var cleanSamplesJobHandle = new CleanSamplesJob
                    {
                        CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                    }.Schedule(this, inputDeps);

                    inputDeps = JobHandle.CombineDependencies(tagBoardDoneJobHandle, cleanSamplesJobHandle);
                    this._endFrameBarrier.AddJobHandleForProducer(inputDeps);

                    inputDeps.Complete();
                    for (var i = 0; i < samplesList.Length; i++)
                        this.InstantiateExit(samplesList[i].Position);

                    samplesArray.Dispose();
                    samplesList.Dispose();
                }
            }
            return inputDeps;
        }

        private JobHandle SetupValidationGrid(JobHandle inputDeps)
        {
            var playerPos = PlayerSpawningSystem.PlayerPosition;
            var playerPosition = new float2(playerPos.x, playerPos.y);
            var initializeValidationGridJobHandle = new InitializeValidationGridJob
            {
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                Tiles = this._tilesGroup.ToComponentDataArray<FinalTileComponent>(Allocator.TempJob),
                PlayerPosition = playerPosition
            }.Schedule(this, inputDeps);
            this._endFrameBarrier.AddJobHandleForProducer(initializeValidationGridJobHandle);
            return initializeValidationGridJobHandle;
        }

        private void InstantiateExit(int2 position)
        {
            var exit = Object.Instantiate(PrefabManager.Instance.DungeonLevelExit,
                new Vector3(position.x + 0.5f, position.y + 0.75f, 0), Quaternion.identity);

            var entity = exit.GetComponent<GameObjectEntity>().Entity;
            EntityManager.AddComponentData(entity, new LevelExitComponent());
            EntityManager.AddComponentData(entity, new PositionComponent {
                CurrentPosition = position,
                InitialPosition = position
            });
        }
    }
}
