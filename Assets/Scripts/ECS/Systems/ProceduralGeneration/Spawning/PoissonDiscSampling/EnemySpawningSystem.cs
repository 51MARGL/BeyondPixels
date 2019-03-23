//using Assets.Scripts.Components.ProceduralGeneration.Dungeon;
//using BeyondPixels.ECS.Components.Characters.AI;
//using BeyondPixels.ECS.Components.Characters.Common;
//using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
//using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling;
//using BeyondPixels.ECS.Systems.ProceduralGeneration.Dungeon.BSP;
//using BeyondPixels.SceneBootstraps;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Mathematics;
//using UnityEngine;

//namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Spawning.PoissonDiscSampling
//{
//    [UpdateAfter(typeof(TileMapSystem))]
//    public class EnemySpawningSystem : JobComponentSystem
//    {
//        private struct EnemiesSpawnStartedComponent : IComponentData { }

//        [DisableAutoCreation]
//        private class EnemySpawningSystemBarrier : EntityCommandBufferSystem { }

//        private struct InitializeValidationGridJob : IJob
//        {
//            public EntityCommandBuffer CommandBuffer;

//            [ReadOnly]
//            public int2 BoardSize;

//            [ReadOnly]
//            public ComponentDataArray<FinalTileComponent> Tiles;

//            public Entity BoardEntity;

//            public void Execute()
//            {
//                var poissonDiscEntity = CommandBuffer.CreateEntity();
//                CommandBuffer.AddComponent(poissonDiscEntity, new PoissonDiscSamplingComponent
//                {
//                    GridSize = BoardSize,
//                    SamplesLimit = 30,
//                    Radius = 7
//                });
//                for (int y = 0; y < BoardSize.y; y++)
//                    for (int x = 0; x < BoardSize.x; x++)
//                    {
//                        var entity = CommandBuffer.CreateEntity();
//                        CommandBuffer.AddComponent(entity, new CellComponent
//                        {
//                            SampleIndex = Tiles[y * BoardSize.x + x].TileType == TileType.Floor ? -1 : -2,
//                            Position = Tiles[y * BoardSize.x + x].Position
//                        });
//                    }

//                CommandBuffer.AddComponent(BoardEntity, new EnemiesSpawnStartedComponent());
//            }
//        }

//        private struct BoardDataStart
//        {
//            public readonly int Length;
//            public ComponentDataArray<FinalBoardComponent> FinalBoardComponents;
//            public ExcludeComponent<EnemiesSpawnStartedComponent> EnemiesSpawnStartedComponents;
//            public EntityArray EntityArray;
//        }
//        [Inject]
//        private BoardDataStart _boardDataStart;

//        private struct BoardDataEnd
//        {
//            public readonly int Length;
//            public ComponentDataArray<FinalBoardComponent> FinalBoardComponents;
//            public ComponentDataArray<EnemiesSpawnStartedComponent> EnemiesSpawnStartedComponents;
//            public ExcludeComponent<EnemiesSpawnedComponent> EnemiesSpawnedComponents;
//            public EntityArray EntityArray;
//        }
//        [Inject]
//        private BoardDataEnd _boardDataEnd;

//        private struct Tiles
//        {
//            public readonly int Length;
//            public ComponentDataArray<FinalTileComponent> TileComponents;
//        }
//        [Inject]
//        private Tiles _tiles;
//        private struct SamplesData
//        {
//            public readonly int Length;
//            public ComponentDataArray<SampleComponent> SampleComponents;
//            public EntityArray EntityArray;
//        }
//        [Inject]
//        private SamplesData _samples;
//        private struct TilemapData
//        {
//            public readonly int Length;
//            public ComponentArray<DungeonTileMapComponent> DungeonTileMapComponents;
//            public ComponentArray<Transform> TransformComponents;
//        }
//        [Inject]
//        private TilemapData _tilemapData;
//        private EnemySpawningSystemBarrier _enemySpawningSystemBarrier;

//        protected override void OnCreateManager()
//        {
//            _enemySpawningSystemBarrier = World.Active.GetOrCreateManager<EnemySpawningSystemBarrier>();
//        }

//        protected override JobHandle OnUpdate(JobHandle inputDeps)
//        {
//            for (int i = 0; i < _boardDataStart.Length; i++)
//                return this.SetupValidationGrid(_boardDataStart.FinalBoardComponents[i].Size, _boardDataStart.EntityArray[i], inputDeps);

//            for (int b = 0; b < _boardDataEnd.Length; b++)
//                for (int t = 0; t < _tilemapData.Length; t++)
//                {
//                    if (_tilemapData.DungeonTileMapComponents[t].tileSpawnRoutine == null && _samples.Length > 0)
//                    {
//                        var commandBuffer = _enemySpawningSystemBarrier.CreateCommandBuffer();
//                        var samplesArray = new NativeArray<SampleComponent>(_samples.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
//                        commandBuffer.AddComponent(_boardDataEnd.EntityArray[b], new EnemiesSpawnedComponent());

//                        for (int i = 0; i < _samples.Length; i++)
//                            samplesArray[i] = _samples.SampleComponents[i];

//                        for (int i = 0; i < _samples.Length; i++)
//                            commandBuffer.DestroyEntity(_samples.EntityArray[i]);

//                        for (int i = 0; i < samplesArray.Length; i++)
//                            InstantiateEnemy(samplesArray[i].Position);

//                        samplesArray.Dispose();
//                    }
//                }
//            return inputDeps;
//        }

//        private JobHandle SetupValidationGrid(int2 boardSize, Entity boardEntity, JobHandle inputDeps)
//        {
//            var initializeValidationGridJobHandle = new InitializeValidationGridJob
//            {
//                CommandBuffer = _enemySpawningSystemBarrier.CreateCommandBuffer(),
//                BoardSize = boardSize,
//                Tiles = _tiles.TileComponents,
//                BoardEntity = boardEntity
//            }.Schedule(inputDeps);
//            _enemySpawningSystemBarrier.AddJobHandleForProducer(initializeValidationGridJobHandle);
//            return initializeValidationGridJobHandle;
//        }

//        private void InstantiateEnemy(int2 position)
//        {
//            var commandBuffer = _enemySpawningSystemBarrier.CreateCommandBuffer();
//            var enemy = Object.Instantiate(PrefabManager.Instance.EnemyPrefab,
//                new Vector3(position.x + 0.5f, position.y + 0.5f, 0), Quaternion.identity);
//            var enemyEntity = enemy.GetComponent<GameObjectEntity>().Entity;
//            var enemyInitializeComponent = enemy.GetComponent<EnemyInitializeComponent>();

//            commandBuffer.AddComponent(enemyEntity, new CharacterComponent
//            {
//                CharacterType = CharacterType.Enemy
//            });
//            commandBuffer.AddComponent(enemyEntity, new MovementComponent
//            {
//                Direction = float2.zero,
//                Speed = enemyInitializeComponent.MovementSpeed
//            });
//            commandBuffer.AddComponent(enemyEntity, new HealthComponent
//            {
//                MaxValue = enemyInitializeComponent.MaxHealth,
//                CurrentValue = enemyInitializeComponent.MaxHealth
//            });
//            commandBuffer.AddComponent(enemyEntity, new WeaponComponent
//            {
//                DamageValue = enemyInitializeComponent.WeaponDamage,
//                AttackRange = enemyInitializeComponent.AttackRange,
//                CoolDown = enemyInitializeComponent.AttackCoolDown
//            });
//            commandBuffer.AddComponent(enemyEntity, new IdleStateComponent
//            {
//                StartedAt = Time.time
//            });
//            commandBuffer.AddComponent(enemyEntity, new PositionComponent
//            {
//                InitialPosition = new float2(enemy.transform.position.x, enemy.transform.position.y)
//            });
//            Object.Destroy(enemyInitializeComponent);
//            commandBuffer.RemoveComponent<EnemyInitializeComponent>(enemyEntity);
//        }
//    }
//}
