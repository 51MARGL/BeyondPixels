using UnityEngine;

namespace BeyondPixels.ECS.Components.Spells
{
    public class SpellBookManagerComponent : MonoBehaviour
    {
        private static SpellBookManagerComponent _instance;

        public static SpellBookManagerComponent Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<SpellBookManagerComponent>();

                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }
        public SpellBookComponent SpellBook { get; private set; }

        public void Start()
        {
            SpellBookManagerComponent.Instance = this;
            this.SpellBook = GetComponent<SpellBookComponent>();
        }
    }
}
