using UnityEngine;

namespace BeyondPixels.ECS.Components.Items
{
    public class ItemsManagerComponent : MonoBehaviour
    {
        public static ItemsManagerComponent Instance { get; private set; }
        public ItemsStoreComponent ItemsStoreComponent { get; private set; }

        public void Awake()
        {
            ItemsManagerComponent.Instance = this;
            this.ItemsStoreComponent = this.GetComponent<ItemsStoreComponent>();
        }
    }
}
