using UnityEngine;

namespace BeyondPixels.ECS.Components.Characters.AI
{
    public class AIInitializeComponent : MonoBehaviour
    {
        public int MovementSpeed;
        public int WeaponDamage;
        public int BaseHealth;
        public float AttackCoolDown;
        public float MeleeAttackRange;
        public float SpellAttackRange;
        public float SpellCheckFrequency;
        public float SpellCastChance;
    }
}
