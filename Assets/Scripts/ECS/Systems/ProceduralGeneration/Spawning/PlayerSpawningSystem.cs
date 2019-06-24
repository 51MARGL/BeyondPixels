using System;

using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning;
using BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling;
using BeyondPixels.ECS.Components.SaveGame;
using BeyondPixels.ECS.Components.Spells;
using BeyondPixels.ECS.Systems.Items;
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
            this._tilesGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(FinalTileComponent)
                }
            });
            this._boardGroup = this.GetComponentGroup(new EntityArchetypeQuery
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
            this._boardReadyGroup = this.GetComponentGroup(new EntityArchetypeQuery
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
            this.Entities.With(this._boardGroup).ForEach((Entity entity, ref FinalBoardComponent finalBoardComponent) =>
            {
                PlayerInstantiated = false;
                PlayerPosition = float3.zero;

                var boardSize = finalBoardComponent.Size;
                var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.ToString("yyyyMMddHHmmssff").GetHashCode());
                var tiles = this._tilesGroup.ToComponentDataArray<FinalTileComponent>(Allocator.TempJob);
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
            if (this._boardReadyGroup.CalculateLength() > 0 && !PlayerInstantiated && !TileMapSystem.TileMapDrawing)
            {
                this.SpawnPlayer(PlayerPosition);
                PlayerInstantiated = true;
            }
        }

        private void SpawnPlayer(float3 position)
        {
            var player = GameObject.Instantiate(PrefabManager.Instance.PlayerPrefab,
                                            position, Quaternion.identity);
        }
    }
}
