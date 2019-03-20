using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.BSP
{
    public struct NodeComponent : IComponentData
    {
        public int IsNull;
        public int IsLeaf;
        public int4 RectBounds;
        public RoomComponent Room;
    }
}
