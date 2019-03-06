using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.CellularAutomaton;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Dungeon.CellularAutomaton
{
    public class TileMapSystem : ComponentSystem
    {
        private struct Data
        {
            public readonly int Length;
            public ComponentDataArray<BoardComponent> BoardComponents;
            public ComponentDataArray<BoardReadyComponent> BoardReadyComponents;
            public SubtractiveComponent<TilemapReadyComponent> TilemapReadyComponents;
            public EntityArray EntityArray;
        }
        [Inject]
        private Data _data;

        private struct TilemapData
        {
            public readonly int Length;
            public ComponentArray<DungeonTileMapComponent> DungeonTileMapComponents;
            public ComponentArray<Transform> TransformComponents;
            public EntityArray EntityArray;
        }
        [Inject]
        private TilemapData _tilemapData;

        private struct Tiles
        {
            public readonly int Length;
            public ComponentDataArray<TileComponent> TileComponents;
        }
        [Inject]
        private Tiles _tiles;
        private NativeList<TileComponent> TilesList;

        protected override void OnCreateManager()
        {
            TilesList = new NativeList<TileComponent>(Allocator.Persistent);
        }

        protected override void OnUpdate()
        {
            for (int i = 0; i < _data.Length; i++)
            {
                for (int j = 0; j < _tilemapData.Length; j++)
                {
                    var tilemapComponent = _tilemapData.DungeonTileMapComponents[j];
                    if (tilemapComponent.tileSpawnRoutine != null)
                        return;

                    TilesList.Clear();
                    for (int k = 0; k < _tiles.Length; k++)
                        TilesList.Add(_tiles.TileComponents[k]);

                    if (TilesList.Length > 0)
                        tilemapComponent.tileSpawnRoutine
                            = tilemapComponent.StartCoroutine(
                                this.SetTiles(j, _data.BoardComponents[i].Size,
                                    _tilemapData.TransformComponents[i]));

                    PostUpdateCommands.AddComponent(_data.EntityArray[i], new TilemapReadyComponent());
                }
            }
        }

        private IEnumerator SetTiles(int entityIndex, int2 boardSize, Transform lightParent)
        {
            var tilemapComponent = _tilemapData.DungeonTileMapComponents[entityIndex];
            var wallCollider = tilemapComponent.TilemapWalls.GetComponent<TilemapCollider2D>();
            wallCollider.enabled = false;
            tilemapComponent.TilemapWalls.ClearAllTiles();
            tilemapComponent.TilemapWallsTop.ClearAllTiles();
            tilemapComponent.TilemapBase.ClearAllTiles();
            tilemapComponent.TilemapWallsAnimated.ClearAllTiles();
            var lightList = new List<Transform>();
            for (int k = 4; k < _tilemapData.TransformComponents[entityIndex].childCount; k++)
                lightList.Add(_tilemapData.TransformComponents[entityIndex].GetChild(k));
            lightList.ForEach(obj => GameObject.Destroy(obj.gameObject));

            var centerX = (int)math.floor(boardSize.x / 2f);
            var centerY = (int)math.floor(boardSize.y / 2f);
            var count = (int)math.ceil(math.max(boardSize.x, boardSize.y) / 2f);

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
                        if (tile.CurrentGenState == TileType.Floor)
                            tilemapComponent.TilemapBase.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.GroundTile);
                        else
                        {
                            tilemapComponent.TilemapWalls.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.WallTile);
                            tilemapComponent.TilemapWallsTop.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.WallTileTop);

                            CreateLightTile(boardSize, lightParent, tilemapComponent, tile);
                        }

                        tile = TilesList[yBottom * boardSize.x + x];
                        if (tile.CurrentGenState == TileType.Floor)
                            tilemapComponent.TilemapBase.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.GroundTile);
                        else
                        {
                            tilemapComponent.TilemapWalls.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.WallTile);
                            tilemapComponent.TilemapWallsTop.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.WallTileTop);

                            CreateLightTile(boardSize, lightParent, tilemapComponent, tile);
                        }

                    }

                if (yBottom >= 0 && yTop < boardSize.y)
                    for (int y = yBottom, iterationCounter = 0; y <= yTop; y++, iterationCounter++)
                    {
                        var tile = TilesList[y * boardSize.x + xLeft];
                        if (tile.CurrentGenState == TileType.Floor)
                            tilemapComponent.TilemapBase.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.GroundTile);
                        else
                        {
                            tilemapComponent.TilemapWalls.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.WallTile);
                            tilemapComponent.TilemapWallsTop.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.WallTileTop);

                            CreateLightTile(boardSize, lightParent, tilemapComponent, tile);
                        }

                        tile = TilesList[y * boardSize.x + xRigth];
                        if (tile.CurrentGenState == TileType.Floor)
                            tilemapComponent.TilemapBase.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.GroundTile);
                        else
                        {
                            tilemapComponent.TilemapWalls.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.WallTile);
                            tilemapComponent.TilemapWallsTop.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.WallTileTop);

                            CreateLightTile(boardSize, lightParent, tilemapComponent, tile);
                        }

                    }
                yield return null;
            }

            if (boardSize.y % 2 == 0)
                for (int x = 0; x < boardSize.x; x++)
                {
                    var tile = TilesList[0 * boardSize.x + x];
                    if (tile.CurrentGenState == TileType.Floor)
                        tilemapComponent.TilemapBase.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.GroundTile);
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
                    if (tile.CurrentGenState == TileType.Floor)
                        tilemapComponent.TilemapBase.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.GroundTile);
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
        }

        private void CreateLightTile(int2 boardSize, Transform lightParent, DungeonTileMapComponent tilemapComponent, TileComponent tile)
        {
            if (tile.Position.x > 1 && tile.Position.x < boardSize.x - 1
                && tile.Position.y > 1 && tile.Position.y < boardSize.y - 1
                && TilesList[(tile.Position.y - 1) * boardSize.x + tile.Position.x].CurrentGenState == TileType.Floor
                && TilesList[(tile.Position.y + 1) * boardSize.x + tile.Position.x].CurrentGenState == TileType.Wall
                && TilesList[(tile.Position.y) * boardSize.x + tile.Position.x + 1].CurrentGenState == TileType.Wall
                && TilesList[(tile.Position.y) * boardSize.x + tile.Position.x - 1].CurrentGenState == TileType.Wall
                && UnityEngine.Random.Range(0, 100) > 75
               )
            {
                tilemapComponent.WallTorchAnimatedTile.m_AnimationStartTime = UnityEngine.Random.Range(1, 10);
                tilemapComponent.TilemapWallsAnimated.SetTile(new Vector3Int(tile.Position.x, tile.Position.y, 0), tilemapComponent.WallTorchAnimatedTile);
                GameObject.Instantiate(tilemapComponent.TorchLight,
                    new Vector3(tile.Position.x + 0.5f, tile.Position.y - 0.5f, -1),
                    Quaternion.identity, lightParent);

            }
        }

        protected override void OnDestroyManager()
        {
            TilesList.Dispose();
        }
    }
}
