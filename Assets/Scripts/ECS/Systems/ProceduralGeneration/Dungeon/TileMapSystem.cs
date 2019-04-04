using System.Collections;
using System.Collections.Generic;
using BeyondPixels.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Dungeon
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class TileMapSystem : ComponentSystem
    {
        public static bool TileMapDrawing = false;
        private NativeList<FinalTileComponent> TilesList;

        private ComponentGroup _tilemapGroup;
        private ComponentGroup _boardGroup;
        private ComponentGroup _tilesGroup;

        protected override void OnCreateManager()
        {
            TilesList = new NativeList<FinalTileComponent>(Allocator.Persistent);
            _boardGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(FinalBoardComponent)
                },
                None = new ComponentType[]
                {
                    typeof(TilemapReadyComponent)
                }
            });
            _tilemapGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(DungeonTileMapComponent), typeof(Transform)
                }
            });
            _tilesGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(FinalTileComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            Entities.With(_boardGroup).ForEach((Entity entity, ref FinalBoardComponent finalBoardComponent) =>
            {
                SetBoardTiles(finalBoardComponent.Size, entity);
            });
        }

        private void SetBoardTiles(int2 boardSize, Entity boardEntity)
        {
            Entities.With(_tilemapGroup).ForEach((Entity entity, Transform transform, DungeonTileMapComponent tilemapComponent) =>
            {
                if (tilemapComponent.tileSpawnRoutine != null)
                    return;

                TileMapSystem.TileMapDrawing = true;
                TilesList.Clear();
                Entities.With(_tilesGroup).ForEach((ref FinalTileComponent finalTileComponent) =>
                {
                    TilesList.Add(finalTileComponent);
                });

                if (TilesList.Length > 0)
                    tilemapComponent.tileSpawnRoutine = tilemapComponent.StartCoroutine(
                                this.SetTiles(entity, tilemapComponent, boardSize, transform)
                            );

                PostUpdateCommands.AddComponent(boardEntity, new TilemapReadyComponent());
            });
        }

        private IEnumerator SetTiles(Entity tilemapEntity, DungeonTileMapComponent tilemapComponent, int2 boardSize, Transform transform)
        {
            var wallCollider = tilemapComponent.TilemapWalls.GetComponent<TilemapCollider2D>();
            wallCollider.enabled = false;
            tilemapComponent.TilemapWalls.ClearAllTiles();
            tilemapComponent.TilemapWallsTop.ClearAllTiles();
            tilemapComponent.TilemapBase.ClearAllTiles();
            tilemapComponent.TilemapWallsAnimated.ClearAllTiles();

            var centerX = (int)math.floor(boardSize.x / 2f);
            var centerY = (int)math.floor(boardSize.y / 2f);
            var count = (int)math.ceil(math.max(boardSize.x, boardSize.y) / 2f);

            yield return null;

            for (int i = 0; i < count; i++)
            {
                var yTop = centerY + i > boardSize.y - 1 ? boardSize.y - 1 : centerY + i;
                var yBottom = centerY - i < 0 ? 0 : centerY - i;
                var xLeft = centerX - i < 0 ? 0 : centerX - i; ;
                var xRigth = centerX + i > boardSize.x - 1 ? boardSize.x - 1 : centerX + i; ;

                if (xLeft >= 0 && xRigth < boardSize.x)
                    for (int x = xLeft, iterationCounter = 0; x <= xRigth; x++, iterationCounter++)
                    {
                        var tile = TilesList[yTop * boardSize.x + x];
                        if (tile.TileType == TileType.Floor)
                        {
                            tilemapComponent.TilemapBase.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.GroundTile);
                            tilemapComponent.TilemapMinimap.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.MinimapTile);
                        }
                        else
                        {
                            tilemapComponent.TilemapWalls.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.WallTile);
                            tilemapComponent.TilemapWallsTop.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.WallTileTop);
                        }

                        tile = TilesList[yBottom * boardSize.x + x];
                        if (tile.TileType == TileType.Floor)
                        {
                            tilemapComponent.TilemapBase.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.GroundTile);
                            tilemapComponent.TilemapMinimap.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.MinimapTile);
                        }
                        else
                        {
                            tilemapComponent.TilemapWalls.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.WallTile);
                            tilemapComponent.TilemapWallsTop.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.WallTileTop);
                        }
                    }

                if (yBottom >= 0 && yTop < boardSize.y)
                    for (int y = yBottom, iterationCounter = 0; y <= yTop; y++, iterationCounter++)
                    {
                        var tile = TilesList[y * boardSize.x + xLeft];
                        if (tile.TileType == TileType.Floor)
                        {
                            tilemapComponent.TilemapBase.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.GroundTile);
                            tilemapComponent.TilemapMinimap.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.MinimapTile);
                        }
                        else
                        {
                            tilemapComponent.TilemapWalls.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.WallTile);
                            tilemapComponent.TilemapWallsTop.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.WallTileTop);
                        }

                        tile = TilesList[y * boardSize.x + xRigth];
                        if (tile.TileType == TileType.Floor)
                        {
                            tilemapComponent.TilemapBase.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.GroundTile);
                            tilemapComponent.TilemapMinimap.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.MinimapTile);
                        }
                        else
                        {
                            tilemapComponent.TilemapWalls.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.WallTile);
                            tilemapComponent.TilemapWallsTop.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.WallTileTop);
                        }
                    }
                yield return null;
            }

            if (boardSize.y % 2 == 0)
                for (int x = 0; x < boardSize.x; x++)
                {
                    var tile = TilesList[0 * boardSize.x + x];
                    if (tile.TileType == TileType.Floor)
                    {
                        tilemapComponent.TilemapBase.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.GroundTile);
                        tilemapComponent.TilemapMinimap.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.MinimapTile);
                    }
                    else
                    {
                        tilemapComponent.TilemapWalls.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.WallTile);
                        tilemapComponent.TilemapWallsTop.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.WallTileTop);
                    }
                }

            if (boardSize.x % 2 == 0)
                for (int y = 0; y < boardSize.y; y++)
                {
                    var tile = TilesList[y * boardSize.x];
                    if (tile.TileType == TileType.Floor)
                    {
                        tilemapComponent.TilemapBase.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.GroundTile);
                        tilemapComponent.TilemapMinimap.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.MinimapTile);
                    }
                    else
                    {
                        tilemapComponent.TilemapWalls.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.WallTile);
                        tilemapComponent.TilemapWallsTop.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.WallTileTop);
                    }
                }
            yield return null;

            //hide skybox
            for (int x = -tilemapComponent.OuterWallWidth; x < boardSize.x + tilemapComponent.OuterWallWidth; x++)
            {
                for (int y = boardSize.y; y < boardSize.y + tilemapComponent.OuterWallWidth; y++)
                {
                    tilemapComponent.TilemapWalls.SetTile(new Vector3Int(x, boardSize.y - 1 - y, 0), tilemapComponent.WallTile);
                    tilemapComponent.TilemapWallsTop.SetTile(new Vector3Int(x, boardSize.y - 1 - y, 0), tilemapComponent.WallTileTop);

                    tilemapComponent.TilemapWalls.SetTile(new Vector3Int(x, y, 0), tilemapComponent.WallTile);
                    tilemapComponent.TilemapWallsTop.SetTile(new Vector3Int(x, y, 0), tilemapComponent.WallTileTop);
                }

                yield return null;
            }
            for (int y = 0; y < boardSize.y; y++)
            {
                for (int x = boardSize.x; x < boardSize.x + tilemapComponent.OuterWallWidth; x++)
                {
                    tilemapComponent.TilemapWalls.SetTile(new Vector3Int(boardSize.x - 1 - x, y, 0), tilemapComponent.WallTile);
                    tilemapComponent.TilemapWallsTop.SetTile(new Vector3Int(boardSize.x - 1 - x, y, 0), tilemapComponent.WallTileTop);

                    tilemapComponent.TilemapWalls.SetTile(new Vector3Int(x, y, 0), tilemapComponent.WallTile);
                    tilemapComponent.TilemapWallsTop.SetTile(new Vector3Int(x, y, 0), tilemapComponent.WallTileTop);
                }

                yield return null;
            }

            TilesList.Clear();
            wallCollider.enabled = true;
            tilemapComponent.tileSpawnRoutine = null;
            TileMapSystem.TileMapDrawing = false;
        }

        protected override void OnDestroyManager()
        {
            TilesList.Dispose();
        }
    }
}
