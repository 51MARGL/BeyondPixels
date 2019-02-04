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
            var objectsToDestroy = new (GameObject gameObject, Entity entity)[count];
            for (int i = 0; i < count; i++)
                objectsToDestroy[i] = (_data.TransformComponents[i].gameObject, _data.EntityArray[i]);

            for (int i = 0; i < count; i++)
            {
                EntityManager.DestroyEntity(objectsToDestroy[i].entity);
                GameObject.Destroy(objectsToDestroy[i].gameObject, 0.01f);
            }

            objectsToDestroy = null;
        }
    }
}
