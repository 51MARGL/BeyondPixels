using Assets.Scripts.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling;
using BeyondPixels.ECS.Systems.ProceduralGeneration.Dungeon.BSP;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Spawning.PoissonDiscSampling
{
    [UpdateAfter(typeof(TileMapSystem))]
    public class LightSpawningSystem : JobComponentSystem
    {
        private struct LightSpawnStartedComponent : IComponentData { }

        [DisableAutoCreation]
        private class LightSpawningSystemBarrier : BarrierSystem { }

        private struct InitializeValidationGridJob : IJob
        {
            public EntityCommandBuffer CommandBuffer;

            [ReadOnly]
            public int2 BoardSize;

            [ReadOnly]
            public ComponentDataArray<FinalTileComponent> Tiles;

            public Entity BoardEntity;

            public void Execute()
            {
                var poissonDiscEntity = CommandBuffer.CreateEntity();
                CommandBuffer.AddComponent(poissonDiscEntity, new PoissonDiscSamplingComponent
                {
                    GridSize = BoardSize,
                    SamplesLimit = 30,
                    Radius = 10
                });
                for (int y = 0; y < BoardSize.y; y++)
                    for (int x = 0; x < BoardSize.x; x++)
                    {
                        var entity = CommandBuffer.CreateEntity();
                        int validationIndex = GetValidationIndex(y * BoardSize.x + x, BoardSize);
                        CommandBuffer.AddComponent(entity, new CellComponent
                        {
                            SampleIndex = validationIndex,
                            Position = Tiles[y * BoardSize.x + x].Position
                        });
                    }
                CommandBuffer.AddComponent(BoardEntity, new LightSpawnStartedComponent());
            }

            private int GetValidationIndex(int tileIndex, int2 boardSize)
            {
                var tile = Tiles[tileIndex];
                if (tile.TileType == TileType.Wall
                    && tile.Position.x > 1 && tile.Position.x < boardSize.x - 1
                    && tile.Position.y > 1 && tile.Position.y < boardSize.y - 1
                    && Tiles[(tile.Position.y - 1) * boardSize.x + tile.Position.x].TileType == TileType.Floor
                    && Tiles[(tile.Position.y + 1) * boardSize.x + tile.Position.x].TileType == TileType.Wall
                    && Tiles[(tile.Position.y) * boardSize.x + tile.Position.x + 1].TileType == TileType.Wall
                    && Tiles[(tile.Position.y) * boardSize.x + tile.Position.x - 1].TileType == TileType.Wall)
                    return -1;

                return -2;
            }
        }

        private struct BoardDataStart
        {
            public readonly int Length;
            public ComponentDataArray<FinalBoardComponent> FinalBoardComponents;
            public ComponentDataArray<EnemiesSpawnedComponent> EnemiesSpawnedComponents;
            public SubtractiveComponent<LightSpawnStartedComponent> LightSpawnStartedComponents;
            public EntityArray EntityArray;
        }
        [Inject]
        private BoardDataStart _boardDataStart;

        private struct BoardDataEnd
        {
            public readonly int Length;
            public ComponentDataArray<FinalBoardComponent> FinalBoardComponents;
            public ComponentDataArray<LightSpawnStartedComponent> LightSpawnStartedComponents;
            public SubtractiveComponent<LightsSpawnedComponent> LightsSpawnedComponents;
            public EntityArray EntityArray;
        }
        [Inject]
        private BoardDataEnd _boardDataEnd;

        private struct Tiles
        {
            public readonly int Length;
            public ComponentDataArray<FinalTileComponent> TileComponents;
        }
        [Inject]
        private Tiles _tiles;
        private struct SamplesData
        {
            public readonly int Length;
            public ComponentDataArray<SampleComponent> SampleComponents;
            public EntityArray EntityArray;
        }
        [Inject]
        private SamplesData _samples;
        private struct TilemapData
        {
            public readonly int Length;
            public ComponentArray<DungeonTileMapComponent> DungeonTileMapComponents;
            public ComponentArray<Transform> TransformComponents;
        }
        [Inject]
        private TilemapData _tilemapData;
        private LightSpawningSystemBarrier _lightSpawningSystemBarrier;

        protected override void OnCreateManager()
        {
            _lightSpawningSystemBarrier = World.Active.GetOrCreateManager<LightSpawningSystemBarrier>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (_boardDataStart.Length > 0)
                return this.SetupValidationGrid(_boardDataStart.FinalBoardComponents[0].Size, _boardDataStart.EntityArray[0], inputDeps);

            for (int b = 0; b < _boardDataEnd.Length; b++)
                for (int t = 0; t < _tilemapData.Length; t++)
                {
                    if (_tilemapData.DungeonTileMapComponents[t].tileSpawnRoutine == null && _samples.Length > 0)
                    {
                        var commandBuffer = _lightSpawningSystemBarrier.CreateCommandBuffer();
                        var samplesArray = new NativeArray<SampleComponent>(_samples.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                        commandBuffer.AddComponent(_boardDataEnd.EntityArray[b], new LightsSpawnedComponent());

                        for (int i = 0; i < _samples.Length; i++)
                            samplesArray[i] = _samples.SampleComponents[i];

                        for (int i = 0; i < _samples.Length; i++)
                            commandBuffer.DestroyEntity(_samples.EntityArray[i]);

                        for (int i = 0; i < samplesArray.Length; i++)
                            InstantiateLights(samplesArray[i].Position,
                                              _tilemapData.DungeonTileMapComponents[t],
                                              _tilemapData.TransformComponents[t]);

                        samplesArray.Dispose();
                    }
                }
            return inputDeps;
        }

        private JobHandle SetupValidationGrid(int2 boardSize, Entity boardEntity, JobHandle inputDeps)
        {
            var initializeValidationGridJobHandle = new InitializeValidationGridJob
            {
                CommandBuffer = _lightSpawningSystemBarrier.CreateCommandBuffer(),
                BoardSize = boardSize,
                Tiles = _tiles.TileComponents,
                BoardEntity = boardEntity
            }.Schedule(inputDeps);
            _lightSpawningSystemBarrier.AddJobHandleForProducer(initializeValidationGridJobHandle);
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
