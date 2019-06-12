using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.Objects;
using Unity.Entities;

namespace BeyondPixels.ECS.Systems.Characters.Common
{
    public class KillSystem : ComponentSystem
    {
        private ComponentGroup _group;

        protected override void OnCreateManager()
        {
            this._group = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(HealthComponent), typeof(CharacterComponent),
                    typeof(PositionComponent), typeof(KilledComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            this.Entities.With(this._group).ForEach((Entity entity, ref HealthComponent healthComponent, ref PositionComponent positionComponent) =>
            {
                this.PostUpdateCommands.AddComponent(entity, new DestroyComponent());
                this.PostUpdateCommands.AddComponent(entity, new DropLootComponent());
                if (EntityManager.HasComponent<XPRewardComponent>(entity))
                    this.PostUpdateCommands.AddComponent(entity, new CollectXPRewardComponent());
            });
        }
    }
}
