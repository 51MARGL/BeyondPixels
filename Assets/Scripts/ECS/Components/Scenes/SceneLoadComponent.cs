using Unity.Entities;

namespace BeyondPixels.ECS.Components.Scenes
{
    public struct SceneLoadComponent : IComponentData
    {
        public int SceneIndex;
    }
}
