using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.BSP
{
    public struct NodeComponent : IComponentData
    {
        public int4 RectBounds;
        public int HasLeftChild;
        public int HasRightChild;
        public RoomComponent Room;

        public int IsLeaf
        {
            get
            {
                return (HasLeftChild == 0) && (HasRightChild == 0) ? 1 : 0;
            }
        }
    }
}
