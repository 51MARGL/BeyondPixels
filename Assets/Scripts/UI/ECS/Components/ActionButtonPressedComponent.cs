using Unity.Entities;

namespace BeyondPixels.UI.ECS.Components
{
    public struct ActionButtonPressedComponent : IComponentData
    {
        public int ActionIndex;
    }
}
