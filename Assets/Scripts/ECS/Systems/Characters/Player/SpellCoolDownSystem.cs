using System.Linq;
using BeyondPixels.ECS.Components.Characters.Common;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Player
{
    public class SpellCoolDownSystem : ComponentSystem
    {
        private struct Data
        {
            public readonly int Length;
            public ComponentArray<SpellBookComponent> SpellBookComponents;
            public EntityArray EntityArray;
        }
        [Inject]
        private Data _data;

        protected override void OnUpdate()
        {
            for (int i = 0; i < _data.Length; i++)
                foreach (var spell in _data.SpellBookComponents[i].Spells.Where(x => x.CoolDownTimeLeft > 0))
                    spell.CoolDownTimeLeft -= Time.deltaTime;
        }
    }
}
