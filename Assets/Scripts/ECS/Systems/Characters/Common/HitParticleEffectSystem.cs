using BeyondPixels.ColliderEvents;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.SceneBootstraps;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Common
{
    [UpdateBefore(typeof(DamageSystem))]
    public class HitParticleEffectSystem : ComponentSystem
    {
        private struct DamageData
        {
            public readonly int Length;
            public ComponentDataArray<CollisionInfo> CollisionInfos;
            public ComponentDataArray<DamageComponent> DamageComponents;
        }
        [Inject]
        private DamageData _damageData;

        private struct Data
        {
            public readonly int Length;
            public ComponentDataArray<CharacterComponent> CharacterComponents;
            public ComponentArray<Transform> TransformComponents;
            public EntityArray EntityArray;
        }
        [Inject]
        private Data _data;

        protected override void OnUpdate()
        {
            var positions = new NativeArray<Vector3>(_damageData.Length, Allocator.TempJob);

            var k = 0;
            for (int i = 0; i < _damageData.Length; i++)
                for (int j = 0; j < _data.Length; j++)
                    if (_damageData.CollisionInfos[i].Other == _data.EntityArray[j] 
                        && _damageData.DamageComponents[i].DamageOnImpact > 0)
                        positions[k++] = _data.TransformComponents[j].position;

            for (int i = 0; i < k; i++)
                GameObject.Instantiate(PrefabManager.Instance.BloodSplashPrefab,
                                       positions[i],
                                       Quaternion.identity);
            positions.Dispose();
        }
    }
}