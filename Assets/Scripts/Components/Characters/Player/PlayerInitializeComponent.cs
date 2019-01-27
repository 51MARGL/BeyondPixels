using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeyondPixels.Components.Characters.Player
{
    public class PlayerInitializeComponent: MonoBehaviour
    {
        public int MovementSpeed;
        public int MaxHealth;
        public int WeaponDamage;
        public string[] AttackComboParams;
    }
}
