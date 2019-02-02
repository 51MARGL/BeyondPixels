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
        public GameObject EnemyPrefab;
        public GameObject BloodSplashPrefab;
    }
}
