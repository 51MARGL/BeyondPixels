using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.Systems.Objects
{
    [UpdateAfter(typeof(DestroySystem))]
    public class GOCleanupSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var goes = GameObject.FindObjectsOfType<GameObjectEntity>();

            foreach (var goe in goes)
                if (!EntityManager.Exists(goe.Entity))
                    GameObject.Destroy(goe.gameObject);
        }
    }
}
