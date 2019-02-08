using Unity.Entities;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.CellularAutomaton
{
    public struct RoomComponent : IComponentData
    {
        public int IsAccessibleFromMainRoom;
        public int IsMainRoom;
        public int TileCount;
    }
}
