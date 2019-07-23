using UnityEngine;
using UnityEngine.Tilemaps;

namespace BeyondPixels.Components.ProceduralGeneration.Dungeon
{
    public class DungeonTileMapComponent : MonoBehaviour
    {
        public Tilemap TilemapBase;
        public Tilemap TilemapWalls;
        public Tilemap TilemapWallsTop;
        public Tilemap TilemapWallsAnimated;
        public Tilemap TilemapMinimap;
        public Tile GroundTile;
        public Tile MinimapTile;
        public RuleTile WallTile;
        public RuleTile WallTileTop;
        public AnimatedTile WallTorchAnimatedTile;
        public GameObject TorchLight;
        public int OuterWallWidth;
        public Coroutine tileSpawnRoutine;
    }
}
