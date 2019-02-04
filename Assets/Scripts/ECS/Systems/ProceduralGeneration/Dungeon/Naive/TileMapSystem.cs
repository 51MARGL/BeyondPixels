using System.Collections.Generic;
using Assets.Scripts.Components.ProceduralGeneration.Dungeon;
using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.Naive;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Dungeon.Naive
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

        protected override void OnUpdate()
        {
            for (int i = 0; i < _data.Length; i++)
            {
                for (int j = 0; j < _tilemapData.Length; j++)
                {
                    var tilemapComponent = _tilemapData.DungeonTileMapComponents[j];

                    tilemapComponent.TilemapWalls.ClearAllTiles();
                    tilemapComponent.TilemapWallsTop.ClearAllTiles();
                    tilemapComponent.TilemapBase.ClearAllTiles();
                    tilemapComponent.TilemapWallsAnimated.ClearAllTiles();
                    var lightList = new List<Transform>();
                    for (int k = 4; k < _tilemapData.TransformComponents[i].childCount; k++)
                        lightList.Add(_tilemapData.TransformComponents[i].GetChild(k));
                    lightList.ForEach(obj => GameObject.Destroy(obj.gameObject));

                    for (int k = 0; k < _tiles.Length; k++)
                    {
                        var tile = _tiles.TileComponents[k];
                        if (tile.TileType == TileType.Floor)
                            tilemapComponent.TilemapBase.SetTile(new Vector3Int(tile.Postition.x, tile.Postition.y, 0), tilemapComponent.GroundTile);
                        else
                        {
                            tilemapComponent.TilemapWalls.SetTile(new Vector3Int(tile.Postition.x, tile.Postition.y, 0), tilemapComponent.WallTile);
                            tilemapComponent.TilemapWallsTop.SetTile(new Vector3Int(tile.Postition.x, tile.Postition.y, 0), tilemapComponent.WallTileTop);
                        }
                    }


                    //hide skybox
                    for (int x = -10; x < _data.BoardComponents[i].Size.x + 10; x++)
                    {
                        for (int y = -10; y < 0; y++)
                        {
                            tilemapComponent.TilemapWalls.SetTile(new Vector3Int(x, y, 0), tilemapComponent.WallTile);
                            tilemapComponent.TilemapWallsTop.SetTile(new Vector3Int(x, y, 0), tilemapComponent.WallTileTop);
                        }
                        for (int y = _data.BoardComponents[i].Size.y; y < _data.BoardComponents[i].Size.y + 10; y++)
                        {
                            tilemapComponent.TilemapWalls.SetTile(new Vector3Int(x, y, 0), tilemapComponent.WallTile);
                            tilemapComponent.TilemapWallsTop.SetTile(new Vector3Int(x, y, 0), tilemapComponent.WallTileTop);
                        }
                    }
                    for (int y = 0; y < _data.BoardComponents[i].Size.y; y++)
                    {
                        for (int x = -10; x < 0; x++)
                        {
                            tilemapComponent.TilemapWalls.SetTile(new Vector3Int(x, y, 0), tilemapComponent.WallTile);
                            tilemapComponent.TilemapWallsTop.SetTile(new Vector3Int(x, y, 0), tilemapComponent.WallTileTop);
                        }
                        for (int x = _data.BoardComponents[i].Size.x; x < _data.BoardComponents[i].Size.x + 10; x++)
                        {
                            tilemapComponent.TilemapWalls.SetTile(new Vector3Int(x, y, 0), tilemapComponent.WallTile);
                            tilemapComponent.TilemapWallsTop.SetTile(new Vector3Int(x, y, 0), tilemapComponent.WallTileTop);
                        }
                    }

                    for (int x = 1; x < tilemapComponent.TilemapWalls.size.x - 1; x++)
                        for (int y = 1; y < tilemapComponent.TilemapWalls.size.y - 1; y++)
                        {
                            var sprite = tilemapComponent.TilemapWalls.GetSprite(new Vector3Int(x, y, 0));
                            if (sprite != null && sprite.name == "wall-fire_0")
                            {
                                tilemapComponent.WallTorchAnimatedTile.m_AnimationStartTime = Random.Range(1, 10);
                                tilemapComponent.TilemapWallsAnimated.SetTile(new Vector3Int(x, y, 0), tilemapComponent.WallTorchAnimatedTile);
                                GameObject.Instantiate(tilemapComponent.TorchLight,
                                    new Vector3(x + 0.5f, y - 0.5f, -1),
                                    Quaternion.identity, _tilemapData.TransformComponents[i]);
                            }
                        }
                }

                PostUpdateCommands.AddComponent(_data.EntityArray[i], new TilemapReadyComponent());
            }
        }
    }
}
