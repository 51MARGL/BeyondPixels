using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.BSP
{
    public struct BoardComponent : IComponentData
    {
        public int2 Size;
        public int MinRoomSize;
        public uint RandomSeed;
    }
}
