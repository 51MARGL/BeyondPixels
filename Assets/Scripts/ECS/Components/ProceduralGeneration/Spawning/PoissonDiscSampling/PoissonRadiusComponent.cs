using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling
{
    public struct PoissonRadiusComponent : IComponentData
    {
        public int Radius;
        public int RequestID;
    }
}