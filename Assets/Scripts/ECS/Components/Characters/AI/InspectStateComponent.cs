﻿using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.ECS.Components.Characters.AI
{
    public struct InspectStateComponent : IComponentData
    {
        public float StartedAt;
        public float2 InspectDirection;
    }
}
