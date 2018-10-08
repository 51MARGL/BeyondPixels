using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapProvider : MonoBehaviour
{
    public Tilemap TilemapBase;
    public Tilemap TilemapWalls;
    public Tilemap TilemapWallsTop;
    public Tilemap TilemapWallsTopAnimated;
    public Tile GroundTile;
    public RuleTile WallTile;
    public RuleTile WallTileTop;
    public AnimatedTile WallTorchAnimated;
    public MapProvider MapProvider { get; set; }

    public void CreateTileMap()
    {
        TilemapWalls.ClearAllTiles();
        TilemapBase.ClearAllTiles();
        for (var x = 0; x < MapProvider.Width; x++)
            for (var y = 0; y < MapProvider.Height; y++)
            {
                if (!MapProvider.Map[x, y])
                {
                    TilemapWalls.SetTile(new Vector3Int(x, y, 0), WallTile);
                    TilemapWallsTop.SetTile(new Vector3Int(x, y, 0), WallTileTop);
                    if (UsefulUtilities.NotOnBorder(x,y, MapProvider.Map)
                        && MapProvider.Map[x, y - 1] && !MapProvider.Map[x, y + 1]
                        && !MapProvider.Map[x - 1, y] && !MapProvider.Map[x + 1, y]
                        && Random.Range(0, 10) > 7)
                    {
                        WallTorchAnimated.m_AnimationStartTime = Random.Range(1, 10);
                        TilemapWallsTopAnimated.SetTile(new Vector3Int(x, y, 0), WallTorchAnimated);
                    }
                }

                TilemapBase.SetTile(new Vector3Int(x, y, 0), GroundTile);
            }
    }
}
