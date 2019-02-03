using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ECS.Components.Characters.Player
{
    public struct InputComponent : IComponentData
    {
        public Vector2 InputDirection;
        public int AttackButtonPressed;
        public int MouseButtonClicked;
        public Vector3 MousePosition;
        public int ActionButtonPressed;
    }
}