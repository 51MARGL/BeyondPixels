using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.CellularAutomaton
{
    public struct BoardComponent : IComponentData
    {
        public int2 Size;
        public int RandomFillPercent;
        public int PassRadius;
    }
}
