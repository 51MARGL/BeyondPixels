using System;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling;
using BeyondPixels.ECS.Components.Spells;
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
                    typeof(EnemiesSpawnedComponent),
                    typeof(FinalBoardComponent)
                },
                None = new ComponentType[]
                {
                    typeof(PlayerSpawnedComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            if (_boardGroup.CalculateLength() == 0)
                return;

            var playerPosition = float3.zero;
            Entities.With(_boardGroup).ForEach((Entity entity, ref FinalBoardComponent finalBoardComponent) =>
            {
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
                            playerPosition = new float3(x + 0.5f, y + 1.75f, 0);

                if (!playerPosition.Equals(float3.zero))
                    PostUpdateCommands.AddComponent(entity, new PlayerSpawnedComponent());
                tiles.Dispose();
            });

            if (playerPosition.Equals(float3.zero))
                return;

            RemoveEnemiesAround(playerPosition, 10);
            SpawnPlayer(playerPosition);
        }

        private void RemoveEnemiesAround(float3 position, int radius)
        {
            var layerMask = LayerMask.GetMask("Enemy");

            var hits = Physics2D.OverlapCircleAll(new Vector2(position.x, position.y),
                                                  radius, layerMask);
            if (hits.Length > 0)
            {
                foreach (var collider in hits)
                {
                    var enemyEntity = collider.GetComponent<GameObjectEntity>().Entity;
                    PostUpdateCommands.AddComponent(enemyEntity, new DestroyComponent());
                }
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
            PostUpdateCommands.AddComponent(playerEntity, new HealthComponent
            {
                MaxValue = playerInitializeComponent.MaxHealth,
                CurrentValue = playerInitializeComponent.MaxHealth
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
        }
    }
}
