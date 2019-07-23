using Unity.Entities;

namespace BeyondPixels.ECS.Components.ProceduralGeneration.Spawning.PoissonDiscSampling
{
    public struct PoissonRadiusComponent : IComponentData
    {
        public int Radius;
        public int RequestID;
    }
}