using Unity.Entities;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Components.Characters.Player
{
    public struct InputComponent : IComponentData
    {
        public float2 InputDirection;
        public int AttackButtonPressed;
        public int MouseButtonClicked;
        public float3 MousePosition;
        public int ActionButtonPressed;
        public int SelectTargetButtonPressed;
    }
}