using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.Naive
{
    public struct TileComponent : IComponentData
    {
        public int2 Position;
        public TileType TileType;
    }
}
