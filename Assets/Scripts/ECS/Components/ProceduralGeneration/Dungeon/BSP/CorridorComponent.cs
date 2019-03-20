using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.BSP
{
    public struct CorridorComponent : IComponentData
    {
        public int2 Start;
        public int2 End;
    }
}
