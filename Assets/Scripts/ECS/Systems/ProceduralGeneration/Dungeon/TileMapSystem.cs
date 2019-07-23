using System.Collections;

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
        private EntityQuery _tilemapGroup;
        private EntityQuery _boardGroup;
        private EntityQuery _tilesGroup;

        protected override void OnCreate()
        {
            this._boardGroup = this.GetEntityQuery(new EntityQueryDesc
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
            this._tilemapGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(DungeonTileMapComponent), typeof(Transform)
                }
            });
            this._tilesGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(FinalTileComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._boardGroup).ForEach((Entity entity, ref FinalBoardComponent finalBoardComponent) =>
            {
                this.SetBoardTiles(finalBoardComponent.Size, entity);
            });
        }

        private void SetBoardTiles(int2 boardSize, Entity boardEntity)
        {
            this.Entities.With(this._tilemapGroup).ForEach((Entity entity, Transform transform, DungeonTileMapComponent tilemapComponent) =>
            {
                if (tilemapComponent.tileSpawnRoutine != null)
                    return;

                var tilesArray = this._tilesGroup.ToComponentDataArray<FinalTileComponent>(Allocator.Persistent);

                tilemapComponent.tileSpawnRoutine = tilemapComponent.StartCoroutine(
                            this.SetTiles(boardEntity, entity, tilemapComponent, boardSize, tilesArray, transform)
                        );

            });
        }

        private IEnumerator SetTiles(Entity boardEntity, Entity tilemapEntity, DungeonTileMapComponent tilemapComponent, int2 boardSize, NativeArray<FinalTileComponent> tilesArray, Transform transform)
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

            for (var i = 0; i < count; i++)
            {
                var yTop = centerY + i > boardSize.y - 1 ? boardSize.y - 1 : centerY + i;
                var yBottom = centerY - i < 0 ? 0 : centerY - i;
                var xLeft = centerX - i < 0 ? 0 : centerX - i; ;
                var xRigth = centerX + i > boardSize.x - 1 ? boardSize.x - 1 : centerX + i; ;

                if (xLeft >= 0 && xRigth < boardSize.x)
                    for (int x = xLeft, iterationCounter = 0; x <= xRigth; x++, iterationCounter++)
                    {
                        var tile = tilesArray[yTop * boardSize.x + x];
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

                        tile = tilesArray[yBottom * boardSize.x + x];
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
                        var tile = tilesArray[y * boardSize.x + xLeft];
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

                        tile = tilesArray[y * boardSize.x + xRigth];
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
                for (var x = 0; x < boardSize.x; x++)
                {
                    var tile = tilesArray[0 * boardSize.x + x];
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
                for (var y = 0; y < boardSize.y; y++)
                {
                    var tile = tilesArray[y * boardSize.x];
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
            for (var x = -tilemapComponent.OuterWallWidth; x < boardSize.x + tilemapComponent.OuterWallWidth; x++)
            {
                for (var y = boardSize.y; y < boardSize.y + tilemapComponent.OuterWallWidth; y++)
                {
                    tilemapComponent.TilemapWalls.SetTile(new Vector3Int(x, boardSize.y - 1 - y, 0), tilemapComponent.WallTile);
                    tilemapComponent.TilemapWallsTop.SetTile(new Vector3Int(x, boardSize.y - 1 - y, 0), tilemapComponent.WallTileTop);

                    tilemapComponent.TilemapWalls.SetTile(new Vector3Int(x, y, 0), tilemapComponent.WallTile);
                    tilemapComponent.TilemapWallsTop.SetTile(new Vector3Int(x, y, 0), tilemapComponent.WallTileTop);
                }

                yield return null;
            }
            for (var y = 0; y < boardSize.y; y++)
            {
                for (var x = boardSize.x; x < boardSize.x + tilemapComponent.OuterWallWidth; x++)
                {
                    tilemapComponent.TilemapWalls.SetTile(new Vector3Int(boardSize.x - 1 - x, y, 0), tilemapComponent.WallTile);
                    tilemapComponent.TilemapWallsTop.SetTile(new Vector3Int(boardSize.x - 1 - x, y, 0), tilemapComponent.WallTileTop);

                    tilemapComponent.TilemapWalls.SetTile(new Vector3Int(x, y, 0), tilemapComponent.WallTile);
                    tilemapComponent.TilemapWallsTop.SetTile(new Vector3Int(x, y, 0), tilemapComponent.WallTileTop);
                }

                yield return null;
            }

            tilesArray.Dispose();
            wallCollider.enabled = true;
            tilemapComponent.tileSpawnRoutine = null;

            var entityManager = World.Active.EntityManager;
            entityManager.AddComponentData(boardEntity, new TilemapReadyComponent());
        }
    }
}
