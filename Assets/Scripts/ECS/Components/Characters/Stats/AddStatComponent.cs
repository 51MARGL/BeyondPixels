using Unity.Entities;

namespace BeyondPixels.ECS.Components.Characters.Stats
{
    public struct AddStatComponent : IComponentData
    {
        public StatTarget StatTarget;
    }
}
