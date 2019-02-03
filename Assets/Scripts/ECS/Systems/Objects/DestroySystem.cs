using System.Collections.Generic;
using BeyondPixels.ECS.Components.Objects;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.ECS.Systems.Objects
{
    public class DestroySystem : ComponentSystem
    {
        private struct Data
        {
            public readonly int Length;
            public ComponentDataArray<DestroyComponent> DestroyComponents;
            public ComponentArray<Transform> TransformComponents;
            public EntityArray EntityArray;
        }
        [Inject]
        private Data _data;

        protected override void OnUpdate()
        {
            var count = _data.Length;
            var objectsToDestroy = new GameObject[count];
            for (int i = 0; i < count; i++)
                objectsToDestroy[i] = _data.TransformComponents[i].gameObject;

            for (int i = 0; i < count; i++)
                GameObject.Destroy(objectsToDestroy[i].gameObject);

            objectsToDestroy = null;
        }
    }
}
