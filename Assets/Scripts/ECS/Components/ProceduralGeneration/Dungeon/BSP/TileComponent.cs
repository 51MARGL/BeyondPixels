using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.BSP
{
    public struct TileComponent : IComponentData
    {
        public int2 Position;
        public TileType TileType;
    }
}
