using System;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling;
using BeyondPixels.SceneBootstraps;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Spawning
{
    public class PlayerSpawningSystem : ComponentSystem
    {
        private EntityQuery _tilesGroup;
        private EntityQuery _boardGroup;
        private EntityQuery _boardReadyGroup;
        private Unity.Mathematics.Random _random;

        protected override void OnCreate()
        {
            this._random = new Unity.Mathematics.Random((uint)System.Guid.NewGuid().GetHashCode());

            this._tilesGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(FinalTileComponent)
                }
            });
            this._boardGroup = this.GetEntityQuery(new EntityQueryDesc
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
            this._boardReadyGroup = this.GetEntityQuery(new EntityQueryDesc
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

        public static float3 PlayerPosition = float3.zero;
        private bool playerInstantiated = false;

        protected override void OnUpdate()
        {
            this.Entities.With(this._boardGroup).ForEach((Entity entity, ref FinalBoardComponent finalBoardComponent) =>
            {
                this.playerInstantiated = false;
                PlayerPosition = float3.zero;

                var boardSize = finalBoardComponent.Size;
                var tiles = this._tilesGroup.ToComponentDataArray<FinalTileComponent>(Allocator.TempJob);
                var bottom = this._random.NextBool();
                var left = this._random.NextBool();
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

                for (var y = startY; yCond(y, endY); y += yStep)
                    for (var x = startX; xCond(x, endX); x += xStep)
                        if (tiles[y * boardSize.x + x].TileType == TileType.Floor
                            && tiles[(y + 1) * boardSize.x + x].TileType == TileType.Wall
                            && tiles[(y + 1) * boardSize.x + (x + 1)].TileType == TileType.Wall
                            && tiles[(y + 1) * boardSize.x + (x - 1)].TileType == TileType.Wall
                            && tiles[y * boardSize.x + (x + 1)].TileType == TileType.Floor
                            && tiles[y * boardSize.x + (x - 1)].TileType == TileType.Floor
                            && tiles[(y - 1) * boardSize.x + x].TileType == TileType.Floor)
                            PlayerPosition = new float3(x + 0.5f, y + 1.75f, 0);

                if (!PlayerPosition.Equals(float3.zero))
                    this.PostUpdateCommands.AddComponent(entity, new PlayerSpawnedComponent());
                tiles.Dispose();
            });
            if (this._boardReadyGroup.CalculateEntityCount() > 0 && !this.playerInstantiated)
            {
                this.SpawnPlayer(PlayerPosition);
                this.playerInstantiated = true;
            }
        }

        private void SpawnPlayer(float3 position)
        {
            var player = GameObject.Instantiate(PrefabManager.Instance.PlayerPrefab,
                                            position, Quaternion.identity);
        }
    }
}
