using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.Naive
{
    public struct RoomComponent : IComponentData
    {
        public int X;
        public int Y;
        public int2 Size;
        public Direction EnteringCorridor;
    }
}
