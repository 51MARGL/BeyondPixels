using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.Utilities
{
    public class AppCloseFix : MonoBehaviour
    {
        private void OnApplicationQuit()
        {
            StopAllCoroutines();

            var entityManager = World.Active.GetOrCreateManager<EntityManager>();
            var entityArray = entityManager.GetAllEntities(Allocator.TempJob);
            for (int i = 0; i < entityArray.Length; i++)
                entityManager.DestroyEntity(entityArray[i]);
            entityArray.Dispose();

            World.Active.Dispose();

            if (!Application.isEditor)
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }
    }
}
