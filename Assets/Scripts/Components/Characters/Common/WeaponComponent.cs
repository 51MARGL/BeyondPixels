﻿using System;
using Unity.Entities;

namespace BeyondPixels.Components.Characters.Common
{
    public struct WeaponComponent : IComponentData
    {
        public int DamageValue;
        public float AttackRange;
        public float CoolDown;
    } 
}
