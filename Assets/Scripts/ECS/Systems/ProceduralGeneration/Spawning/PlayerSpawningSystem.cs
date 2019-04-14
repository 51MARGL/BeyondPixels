using System;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling;
using BeyondPixels.ECS.Components.SaveGame;
using BeyondPixels.ECS.Components.Spells;
using BeyondPixels.ECS.Systems.ProceduralGeneration.Dungeon;
using BeyondPixels.SceneBootstraps;
using Cinemachine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Spawning
{
    public class PlayerSpawningSystem : ComponentSystem
    {
        private ComponentGroup _tilesGroup;
        private ComponentGroup _boardGroup;
        private ComponentGroup _boardReadyGroup;

        protected override void OnCreateManager()
        {
            _tilesGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(FinalTileComponent)
                }
            });
            _boardGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(FinalBoardComponent),
                    typeof(TilemapReadyComponent)
                },
                None = new ComponentType[]
                {
                    typeof(PlayerSpawnedComponent)
                }
            });
            _boardReadyGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(FinalBoardComponent),
                    typeof(PlayerSpawnedComponent),
                    typeof(TilemapReadyComponent)
                },
                None = new ComponentType[]
                {
                    typeof(EnemiesSpawnedComponent)
                }
            });
        }

        public static bool PlayerInstantiated = false;
        public static float3 PlayerPosition = float3.zero;

        protected override void OnUpdate()
        {
            Entities.With(_boardGroup).ForEach((Entity entity, ref FinalBoardComponent finalBoardComponent) =>
            {
                PlayerInstantiated = false;
                PlayerPosition = float3.zero;

                var boardSize = finalBoardComponent.Size;
                var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.ToString("yyyyMMddHHmmssff").GetHashCode());
                var tiles = _tilesGroup.ToComponentDataArray<FinalTileComponent>(Allocator.TempJob);
                var bottom = random.NextBool();
                var left = random.NextBool();
                var startX = 3;
                var endX = boardSize.x - 3;
                var startY = 3;
                var endY = boardSize.y - 3;
                var xStep = 1;
                var yStep = 1;

                Func<int, int, bool> yCond = (int current, int end) => current < end;
                Func<int, int, bool> xCond = (int current, int end) => current < end;

                if (!bottom)
                {
                    yStep = -1;
                    startY = boardSize.y - 3;
                    endY = 3;

                    yCond = (int current, int end) => current > end;
                }
                if (!left)
                {
                    xStep = -1;
                    startX = boardSize.x - 3;
                    endX = 3;

                    xCond = (int current, int end) => current > end;
                }

                for (int y = startY; yCond(y, endY); y += yStep)
                    for (int x = startX; xCond(x, endX); x += xStep)
                        if (tiles[y * boardSize.x + x].TileType == TileType.Floor
                            && tiles[(y + 1) * boardSize.x + x].TileType == TileType.Wall
                            && tiles[(y + 1) * boardSize.x + (x + 1)].TileType == TileType.Wall
                            && tiles[(y + 1) * boardSize.x + (x - 1)].TileType == TileType.Wall
                            && tiles[y * boardSize.x + (x + 1)].TileType == TileType.Floor
                            && tiles[y * boardSize.x + (x - 1)].TileType == TileType.Floor
                            && tiles[(y - 1) * boardSize.x + x].TileType == TileType.Floor)
                            PlayerPosition = new float3(x + 0.5f, y + 1.75f, 0);

                if (!PlayerPosition.Equals(float3.zero))
                    PostUpdateCommands.AddComponent(entity, new PlayerSpawnedComponent());
                tiles.Dispose();
            });
            if (_boardReadyGroup.CalculateLength() > 0 && !PlayerInstantiated && !TileMapSystem.TileMapDrawing)
            {
                SpawnPlayer(PlayerPosition);
                var loadGameEntity = PostUpdateCommands.CreateEntity();
                PostUpdateCommands.AddComponent(loadGameEntity, new LoadGameComponent());
                PlayerInstantiated = true;
            }
        }

        private void SpawnPlayer(float3 position)
        {
            #region PlayerEntityArchetype
            var player = GameObject.Instantiate(PrefabManager.Instance.PlayerPrefab,
                                            position, Quaternion.identity);
            var playerEntity = player.GetComponent<GameObjectEntity>().Entity;
            var playerInitializeComponent = player.GetComponent<PlayerInitializeComponent>();
            PostUpdateCommands.AddComponent(playerEntity, new PlayerComponent());
            PostUpdateCommands.AddComponent(playerEntity, new InputComponent());

            PostUpdateCommands.AddComponent(playerEntity, new CharacterComponent
            {
                CharacterType = CharacterType.Player
            });
            PostUpdateCommands.AddComponent(playerEntity, new MovementComponent
            {
                Direction = float2.zero,
                Speed = playerInitializeComponent.MovementSpeed
            });
            PostUpdateCommands.AddComponent(playerEntity, new WeaponComponent
            {
                DamageValue = playerInitializeComponent.WeaponDamage
            });
            PostUpdateCommands.AddComponent(playerEntity, new PositionComponent
            {
                InitialPosition = new float2(player.transform.position.x, player.transform.position.y)
            });
            GameObject.Destroy(playerInitializeComponent);
            PostUpdateCommands.RemoveComponent<PlayerInitializeComponent>(playerEntity);

            #region statsInit
            var statsInitializeComponent = player.GetComponent<StatsInitializeComponent>();
            PostUpdateCommands.AddComponent(playerEntity, statsInitializeComponent.LevelComponent);
            PostUpdateCommands.AddComponent(playerEntity, statsInitializeComponent.HealthStatComponent);
            PostUpdateCommands.AddComponent(playerEntity, new HealthComponent
            {
                MaxValue = statsInitializeComponent.HealthStatComponent.CurrentValue,
                CurrentValue = statsInitializeComponent.HealthStatComponent.CurrentValue
            });
            PostUpdateCommands.AddComponent(playerEntity, statsInitializeComponent.AttackStatComponent);
            PostUpdateCommands.AddComponent(playerEntity, statsInitializeComponent.DefenceStatComponent);
            PostUpdateCommands.AddComponent(playerEntity, statsInitializeComponent.MagicStatComponent);
            PostUpdateCommands.AddComponent(playerEntity, new XPComponent());
            PostUpdateCommands.AddComponent(playerEntity, new AdjustStatsComponent());
            GameObject.Destroy(statsInitializeComponent);
            #endregion
            #endregion

            #region spellInit
            for (int i = 0; i < 3; i++)
            {
                var spellEntity = PostUpdateCommands.CreateEntity();
                PostUpdateCommands.AddComponent(spellEntity, new ActiveSpellComponent
                {
                    Owner = playerEntity,
                    ActionIndex = i + 1,
                    SpellIndex = i
                });
            }
            #endregion

            #region camera
            var camera = GameObject.Find("PlayerVCamera").GetComponent<CinemachineVirtualCamera>();
            camera.Follow = player.transform;
            camera.LookAt = player.transform;
            #endregion


            PostUpdateCommands.AddComponent(playerEntity, new InCutsceneComponent());
        }
    }
}
