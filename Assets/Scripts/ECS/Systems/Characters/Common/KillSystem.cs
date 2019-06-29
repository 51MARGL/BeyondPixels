using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.ECS.Components.Scenes;
using Unity.Entities;

namespace BeyondPixels.ECS.Systems.Characters.Common
{
    public class KillSystem : ComponentSystem
    {
        private EntityQuery _group;

        protected override void OnCreate()
        {
            this._group = this.GetEntityQuery(new EntityQueryDesc
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
            this.Entities.With(this._group).ForEach((Entity entity, ref HealthComponent healthComponent, ref CharacterComponent characterComponent) =>
            {
                this.PostUpdateCommands.AddComponent(entity, new DestroyComponent());
                if (characterComponent.CharacterType == CharacterType.Player)
                {
                    var gameOverEntity = this.PostUpdateCommands.CreateEntity();
                    this.PostUpdateCommands.AddComponent(gameOverEntity, new GameOverComponent());
                }
                else
                {
                    this.PostUpdateCommands.AddComponent(entity, new DropLootComponent());
                    if (EntityManager.HasComponent<XPRewardComponent>(entity))
                        this.PostUpdateCommands.AddComponent(entity, new CollectXPRewardComponent());
                }
            });
        }
    }
}
