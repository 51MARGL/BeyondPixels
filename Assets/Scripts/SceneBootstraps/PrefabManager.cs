using System;

using UnityEngine;

namespace BeyondPixels.SceneBootstraps
{
    public class PrefabManager : MonoBehaviour
    {
        public static PrefabManager Instance { get; private set; }

        public GameObject PlayerPrefab;
        public GameObject LevelUpEffectPrefab;
        public GameObject BloodSplashPrefab;
        public GameObject[] BloodDecalsPrefabs;
        public GameObject DungeonLevelEnter;
        public GameObject DungeonLevelExit;
        public GameObject LootBag;
        public GameObject Chest;
        public GameObject Cage;
        public GameObject Ally;
        public EnemyPrefab[] EnemyPrefabs;

        [Serializable]
        public class EnemyPrefab
        {
            public int SpawnRadius;
            public GameObject Prefab;
        }

        public void Start()
        {
            PrefabManager.Instance = this;
        }
    }
}
