using System;

using BeyondPixels.ECS.Components.Characters.Common;

using UnityEngine;

namespace BeyondPixels.ECS.Components.Spells
{
    public class SpellBookComponent : MonoBehaviour
    {
        public Spell[] Spells;

        [Serializable]
        public class Spell
        {
            public GameObject Prefab;

            public Color BarColor;
            public Sprite Icon;
            public string Name;
            public string Description;

            public float ThrowSpeed;
            public float CastTime;
            public float Duration;
            public float CoolDown;
            public int DamageOnImpact;
            public int DamagePerSecond;
            public DamageType DamageType;
            public bool LockOnTarget;
            public bool SelfTarget;
            public bool TargetRequired;
            public bool Throwable;            
        }
    }
}
