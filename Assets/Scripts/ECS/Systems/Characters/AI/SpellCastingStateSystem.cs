using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Spells;

using Unity.Entities;

namespace BeyondPixels.ECS.Systems.Characters.AI
{
    public class SpellCastingStateSystem : ComponentSystem
    {
        private EntityQuery _castingGroup;
        private EntityQuery _activeSpellGroup;

        protected override void OnCreate()
        {
            this._castingGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(PositionComponent), typeof(FollowStateComponent), typeof(SpellCastingComponent),
                    typeof(MagicStatComponent)
                },
                None = new ComponentType[]
                {
                    typeof(AttackStateComponent)
                }
            });
            this._activeSpellGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    ComponentType.ReadOnly<ActiveSpellComponent>()
                },
                None = new ComponentType[]
                {
                    ComponentType.ReadOnly<CoolDownComponent>(),
                    ComponentType.ReadOnly<InstantiateSpellComponent>()
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._castingGroup).ForEach((Entity entity,
                                                ref PositionComponent positionComponent,
                                                ref FollowStateComponent followStateComponent,
                                                ref SpellCastingComponent spellCastingComponent,
                                                ref MagicStatComponent magicStatComponent) =>
            {
                if (!this.EntityManager.Exists(followStateComponent.Target)
                    || this.EntityManager.HasComponent<InCutsceneComponent>(followStateComponent.Target))
                {
                    this.PostUpdateCommands.RemoveComponent<SpellCastingComponent>(entity);
                    return;
                }
            });
        }
    }
}
