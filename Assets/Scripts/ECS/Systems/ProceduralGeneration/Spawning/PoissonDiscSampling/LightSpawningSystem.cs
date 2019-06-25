using BeyondPixels.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling;
using BeyondPixels.ECS.Systems.ProceduralGeneration.Dungeon;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Spawning.PoissonDiscSampling
{
    public class LightSpawningSystem : JobComponentSystem
    {
        private const int SystemRequestID = 2;
        private struct LightSpawnStartedComponent : IComponentData { }

        private struct InitializeValidationGridJob : IJobForEachWithEntity<FinalBoardComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<FinalTileComponent> Tiles;

            public void Execute(Entity boardEntity, int index, ref FinalBoardComponent finalBoardComponent)
            {
                var poissonDiscEntity = this.CommandBuffer.CreateEntity(index);
                var boardSize = finalBoardComponent.Size;
                this.CommandBuffer.AddComponent(index, poissonDiscEntity, new PoissonDiscSamplingComponent
                {
                    GridSize = boardSize,
                    SamplesLimit = 30,
                    Radius = 10,
                    RequestID = SystemRequestID
                });
                for (var y = 0; y < boardSize.y; y++)
                    for (var x = 0; x < boardSize.x; x++)
                    {
                        var entity = this.CommandBuffer.CreateEntity(index);
                        var validationIndex = this.GetValidationIndex(y * boardSize.x + x, boardSize);
                        this.CommandBuffer.AddComponent(index, entity, new PoissonCellComponent
                        {
                            SampleIndex = validationIndex,
                            Position = this.Tiles[y * boardSize.x + x].Position,
                            RequestID = SystemRequestID
                        });
                    }
                this.CommandBuffer.AddComponent(index, boardEntity, new LightSpawnStartedComponent());
            }

            private int GetValidationIndex(int tileIndex, int2 boardSize)
            {
                var tile = this.Tiles[tileIndex];
                if (tile.TileType == TileType.Wall
                    && tile.Position.x > 1 && tile.Position.x < boardSize.x - 1
                    && tile.Position.y > 1 && tile.Position.y < boardSize.y - 1
                    && this.Tiles[(tile.Position.y - 1) * boardSize.x + tile.Position.x].TileType == TileType.Floor
                    && this.Tiles[(tile.Position.y + 1) * boardSize.x + tile.Position.x].TileType == TileType.Wall
                    && this.Tiles[(tile.Position.y) * boardSize.x + tile.Position.x + 1].TileType == TileType.Wall
                    && this.Tiles[(tile.Position.y) * boardSize.x + tile.Position.x - 1].TileType == TileType.Wall)
                    return -1;

                return -2;
            }
        }

        private struct TagBoardDoneJob : IJobForEachWithEntity<FinalBoardComponent>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity, int index, [ReadOnly] ref FinalBoardComponent finalBoardComponent)
            {
                this.CommandBuffer.AddComponent(index, entity, new LightsSpawnedComponent());
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
        private EntityQuery _tileMapGroup;

        protected override void OnCreateManager()
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
                    typeof(FinalBoardComponent)
                },
                None = new ComponentType[]
                {
                    typeof(LightSpawnStartedComponent)
                }
            });
            this._boardSpawnReadyGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(FinalBoardComponent), typeof(LightSpawnStartedComponent)
                },
                None = new ComponentType[]
                {
                    typeof(LightsSpawnedComponent)
                }
            });
            this._samplesGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(SampleComponent)
                }
            });
            this._tileMapGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(DungeonTileMapComponent),
                    typeof(Transform)
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (this._boardSpawnInitGroup.CalculateLength() > 0)
                return this.SetupValidationGrid(inputDeps);

            if (this._boardSpawnReadyGroup.CalculateLength() > 0)
            {
                if (!TileMapSystem.TileMapDrawing && this._samplesGroup.CalculateLength() > 0)
                {
                    var tileMapComponent = this._tileMapGroup.ToComponentArray<DungeonTileMapComponent>()[0];
                    var tileMapTransform = this._tileMapGroup.ToComponentArray<Transform>()[0];

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
                        this.InstantiateLights(samplesList[i].Position,
                                        tileMapComponent,
                                        tileMapTransform);

                    samplesArray.Dispose();
                    samplesList.Dispose();
                }
            }
            return inputDeps;
        }

        private JobHandle SetupValidationGrid(JobHandle inputDeps)
        {
            var initializeValidationGridJobHandle = new InitializeValidationGridJob
            {
                CommandBuffer = this._endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
                Tiles = this._tilesGroup.ToComponentDataArray<FinalTileComponent>(Allocator.TempJob),
            }.Schedule(this, inputDeps);
            this._endFrameBarrier.AddJobHandleForProducer(initializeValidationGridJobHandle);
            return initializeValidationGridJobHandle;
        }

        private void InstantiateLights(int2 position, DungeonTileMapComponent tilemapComponent, Transform lightParent)
        {
            tilemapComponent.WallTorchAnimatedTile.m_AnimationStartTime = UnityEngine.Random.Range(1, 10);
            tilemapComponent.TilemapWallsAnimated.SetTile(new Vector3Int(position.x, position.y, 0), tilemapComponent.WallTorchAnimatedTile);
            Object.Instantiate(tilemapComponent.TorchLight,
                new Vector3(position.x + 0.5f, position.y - 0.5f, -1),
                Quaternion.identity, lightParent);
        }
    }
}
