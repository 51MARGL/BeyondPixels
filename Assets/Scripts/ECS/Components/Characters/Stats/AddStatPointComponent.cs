using Unity.Entities;

namespace BeyondPixels.ECS.Components.Characters.Stats
{
    public struct AddStatPointComponent : IComponentData
    {
        public StatTarget StatTarget;
    }
}
