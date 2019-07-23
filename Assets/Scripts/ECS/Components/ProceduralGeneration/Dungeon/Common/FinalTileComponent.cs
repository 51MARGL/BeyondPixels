using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon
{
    public struct FinalTileComponent : IComponentData
    {
        public int2 Position;
        public TileType TileType;
    }

    public enum TileType
    {
        Wall = 0,
        Floor = 1
    }
}
