using System;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ECS.Components.Characters.Common
{
    public struct HealthComponent : IComponentData
    {
        public int MaxValue;
        public int CurrentValue;
    } 
}
