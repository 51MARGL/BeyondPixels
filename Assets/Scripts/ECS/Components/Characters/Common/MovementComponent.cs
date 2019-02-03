using System;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ECS.Components.Characters.Common
{
    public struct MovementComponent : IComponentData
    {
        public Vector2 Direction;
        public float Speed;
    } 
}
