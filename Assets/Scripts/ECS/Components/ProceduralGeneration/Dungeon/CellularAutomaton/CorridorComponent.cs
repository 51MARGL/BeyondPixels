using Unity.Entities;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.CellularAutomaton
{
    public struct CorridorComponent : IComponentData
    {
        public TileComponent Start;
        public TileComponent End;
    }
}
