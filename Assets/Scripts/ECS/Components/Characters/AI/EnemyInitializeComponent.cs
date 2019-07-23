using UnityEngine;

namespace BeyondPixels.ECS.Components.Characters.AI
{
    public class EnemyInitializeComponent : MonoBehaviour
    {
        public int MovementSpeed;
        public int WeaponDamage;
        public int BaseHealth;
        public float AttackCoolDown;
        public float AttackRange;
    }
}
