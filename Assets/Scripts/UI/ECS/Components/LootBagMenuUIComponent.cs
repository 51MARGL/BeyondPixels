using BeyondPixels.UI.Menus;

using System;

using static BeyondPixels.UI.ECS.Components.PlayerInfoMenuUIComponent;

namespace BeyondPixels.UI.ECS.Components
{
    public class LootBagMenuUIComponent : MenuUI
    {
        public LootGroupWrapper LootGroup;

        [Serializable]
        public class LootGroupWrapper : InventoryGroupWrapper { }
    }
}
