using UnityEngine;

namespace BeyondPixels.ECS.Components.Spells
{
    public class SpellBookManagerComponent : MonoBehaviour
    {
        public static SpellBookManagerComponent Instance { get; private set; }
        public SpellBookComponent SpellBook { get; private set; }

        public void Start()
        {
            SpellBookManagerComponent.Instance = this;
            this.SpellBook = GetComponent<SpellBookComponent>();
        }
    }
}
