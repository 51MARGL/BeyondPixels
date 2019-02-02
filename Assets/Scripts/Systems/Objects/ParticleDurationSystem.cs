using BeyondPixels.Components.Objects;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.Systems.Objects
{
    [UpdateBefore(typeof(DestroySystem))]
    public class ParticleDurationSystem : ComponentSystem
    {
        private struct Data
        {
            public readonly int Length;
            public ComponentArray<ParticleSystem> ParticleSystemComponents;
            public SubtractiveComponent<DestroyComponent> DestroyComponents;
            public EntityArray EntityArray;
        }
        [Inject]
        private Data _data;

        protected override void OnUpdate()
        {
            for (int i = 0; i < _data.Length; i++)
                if (!_data.ParticleSystemComponents[i].IsAlive())
                    PostUpdateCommands.AddComponent(_data.EntityArray[i], new DestroyComponent());
        }
    }
}
