using Unity.Entities;

namespace BeyondPixels.ECS.Components.Objects
{
    public struct SyncDestroyedComponent : IComponentData
    {
        public int EntityID;
    }
}
