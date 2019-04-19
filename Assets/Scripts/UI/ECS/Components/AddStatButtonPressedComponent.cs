using BeyondPixels.ECS.Components.Characters.Stats;
using Unity.Entities;

namespace BeyondPixels.UI.ECS.Components
{
    public struct AddStatButtonPressedComponent : IComponentData
    {
        public StatTarget StatTarget;
    }
}
