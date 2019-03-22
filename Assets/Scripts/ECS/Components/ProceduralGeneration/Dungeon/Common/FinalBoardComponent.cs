using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon
{
    public struct FinalBoardComponent : IComponentData {
        public int2 Size;
    }
}
