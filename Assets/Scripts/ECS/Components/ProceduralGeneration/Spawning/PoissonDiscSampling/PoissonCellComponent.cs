using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling
{
    public struct PoissonCellComponent : IComponentData
    {
        public int2 Position;
        public int SampleIndex;
        public int RequestID;
    }
}