using Unity.Entities;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.CellularAutomaton
{
    public struct RoomComponent : IComponentData
    {
        public int IsAccessibleFromMainRoom;
        public int StartTileIndex;
        public int TileCount;
        public int RoomArrayIndex;
    }
}
