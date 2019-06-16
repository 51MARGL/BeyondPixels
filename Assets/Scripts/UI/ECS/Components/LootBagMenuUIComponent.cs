using System;

using UnityEngine;
using static BeyondPixels.UI.ECS.Components.PlayerInfoMenuUIComponent;

namespace BeyondPixels.UI.ECS.Components
{
    public class LootBagMenuUIComponent : MonoBehaviour
    {
        public LootGroupWrapper LootGroup;

        [Serializable]
        public class LootGroupWrapper : InventoryGroupWrapper { }
    }
}
