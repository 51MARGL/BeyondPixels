using UnityEngine;

namespace BeyondPixels.ECS.Components.Characters.AI
{
    public class EnemyInitializeComponent : MonoBehaviour
    {
        public int MovementSpeed;
        public int MaxHealth;
        public int WeaponDamage;
        public float AttackCoolDown;
        public float AttackRange;
    }
}
