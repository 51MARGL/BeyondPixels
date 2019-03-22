using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling
{
    public struct SampleComponent : IComponentData
    {
        public int2 Position;
        public int Radius;
    }
}