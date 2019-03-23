using System.Linq;
using BeyondPixels.ECS.Components.Characters.Common;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Player
{
    public class SpellCoolDownSystem : ComponentSystem
    {
       private ComponentGroup _group;

        protected override void OnCreateManager()
        {
            _group = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(SpellBookComponent), typeof(PositionComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            Entities.With(_group).ForEach((SpellBookComponent spellBookComponent) =>
            {
                foreach (var spell in spellBookComponent.Spells.Where(x => x.CoolDownTimeLeft > 0))
                    spell.CoolDownTimeLeft -= Time.deltaTime;
            });
        }
    }
}
