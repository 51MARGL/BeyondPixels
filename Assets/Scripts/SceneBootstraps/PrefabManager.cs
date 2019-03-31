using System;
using UnityEngine;

namespace BeyondPixels.SceneBootstraps
{
    public class PrefabManager : MonoBehaviour
    {
        private static PrefabManager _instance;
        public static PrefabManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<PrefabManager>();

                return _instance;
            }
        }

        public GameObject PlayerPrefab;
        public GameObject BloodSplashPrefab;
        public GameObject DungeonLevelEnter;
        public EnemyPrefab[] EnemyPrefabs;

        [Serializable]
        public class EnemyPrefab
        {
            public int SpawnRadius;
            public GameObject Prefab;
        }
    }
}
