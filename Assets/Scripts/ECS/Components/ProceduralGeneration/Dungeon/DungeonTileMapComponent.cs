using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Components.ProceduralGeneration.Dungeon
{
    public class DungeonTileMapComponent : MonoBehaviour
    {
        public Tilemap TilemapBase;
        public Tilemap TilemapWalls;
        public Tilemap TilemapWallsTop;
        public Tilemap TilemapWallsAnimated;
        public Tile GroundTile;
        public RuleTile WallTile;
        public RuleTile WallTileTop;
        public AnimatedTile WallTorchAnimatedTile;
        public GameObject TorchLight;
    }
}
