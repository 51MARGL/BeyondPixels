using System;
using UnityEngine;

namespace BeyondPixels.ECS.Components.Characters.Common
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

            public float CastTime;
            public float Duration;
            public float CoolDown;
            public int DamageOnImpact;
            public int DamagePerSecond;
            public DamageType DamageType;
            public bool LockOnTarget;
            public bool SelfTarget;
            public bool TargetRequired;

            public float CoolDownTimeLeft;
        }
    }
}
