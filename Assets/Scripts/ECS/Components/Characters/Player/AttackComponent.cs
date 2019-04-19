using Unity.Entities;

namespace BeyondPixels.ECS.Components.Characters.Player
{
    public struct AttackComponent : IComponentData
    {
        public int CurrentComboIndex;
    }
}
