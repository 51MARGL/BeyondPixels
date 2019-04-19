﻿using Unity.Entities;

namespace BeyondPixels.ECS.Components.Characters.Common
{
    public struct WeaponComponent : IComponentData
    {
        public int DamageValue;
        public float AttackRange;
        public float CoolDown;
    }
}
