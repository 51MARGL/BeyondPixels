using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.SceneBootstraps;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Characters.Level
{
    [UpdateBefore(typeof(LevelUpSystem))]
    public class LevelUpEffectSystem : ComponentSystem
    {
        private ComponentGroup _characterGroup;

        protected override void OnCreateManager()
        {
            this._characterGroup = this.GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]
                {
                    typeof(LevelComponent), typeof(LevelUpComponent),
                    typeof(PositionComponent), typeof(PlayerComponent)
                }
            });
        }
        protected override void OnUpdate()
        {
            if (this._characterGroup.CalculateLength() != 0)
            {
                var positions = this._characterGroup.ToComponentDataArray<PositionComponent>(Allocator.TempJob);

                for (var i = 0; i < positions.Length; i++)
                    GameObject.Instantiate(PrefabManager.Instance.LevelUpEffectPrefab,
                                           new float3(positions[i].CurrentPosition.x, positions[i].CurrentPosition.y + 0.8f, 0),
                                           Quaternion.Euler(90, 0, 0));

                positions.Dispose();
            }
        }
    }
}
