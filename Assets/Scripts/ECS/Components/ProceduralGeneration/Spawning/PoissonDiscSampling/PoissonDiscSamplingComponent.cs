using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling
{
    public struct PoissonDiscSamplingComponent : IComponentData
    {
        public int2 GridSize;
        public int SamplesLimit;
        public int Radius;
        public int HorizontalSearch;
    }
}
