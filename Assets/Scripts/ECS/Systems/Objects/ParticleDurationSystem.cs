using BeyondPixels.ECS.Components.Objects;

using Unity.Entities;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Objects
{
    [UpdateBefore(typeof(DestroySystem))]
    public class ParticleDurationSystem : ComponentSystem
    {
        private EntityQuery _particlesGroup;

        protected override void OnCreateManager()
        {
            this._particlesGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(ParticleSystem)
                },
                None = new ComponentType[]
                {
                    typeof(DestroyComponent)
                }
            });
        }
        protected override void OnUpdate()
        {
            this.Entities.With(this._particlesGroup).ForEach((Entity entity, ParticleSystem particleSystem) =>
            {
                if (particleSystem != null && !particleSystem.IsAlive())
                    this.PostUpdateCommands.AddComponent(entity, new DestroyComponent());
            });
        }
    }
}
