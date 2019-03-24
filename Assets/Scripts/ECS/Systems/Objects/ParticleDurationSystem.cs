using BeyondPixels.ECS.Components.Objects;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Objects
{
    [UpdateBefore(typeof(DestroySystem))]
    public class ParticleDurationSystem : ComponentSystem
    {
        private ComponentGroup _particlesGroup;

        protected override void OnCreateManager()
        {
            _particlesGroup = GetComponentGroup(new EntityArchetypeQuery
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
            Entities.With(_particlesGroup).ForEach((Entity entity, ParticleSystem particleSystem) =>
            {
                if (!particleSystem.IsAlive())
                    PostUpdateCommands.AddComponent(entity, new DestroyComponent());
            });
        }
    }
}
