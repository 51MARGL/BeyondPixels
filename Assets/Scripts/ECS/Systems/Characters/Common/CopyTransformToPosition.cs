using BeyondPixels.ECS.Components.Characters.Common;

using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine.Jobs;

namespace BeyondPixels.ECS.Systems.Characters.Common
{
    public class CopyTransformToPosition : ComponentSystem
    {
        private ComponentGroup _transformGroup;

        protected override void OnCreateManager()
        {
            this._transformGroup = this.GetComponentGroup(typeof(PositionComponent), typeof(UnityEngine.Transform));
        }

        protected override void OnUpdate()
        {
            Entities.With(_transformGroup).ForEach((UnityEngine.Transform transform, ref PositionComponent positionComponent) => {
                positionComponent.CurrentPosition = new float2(transform.position.x, transform.position.y);
            });
        }
    }
}
