using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.CellularAutomaton
{
    public struct TileComponent : IComponentData
    {
        public int2 Position;
        public TileType CurrentGenState;
        public TileType NextGenState;
    }
}
