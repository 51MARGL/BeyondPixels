using System;
using UnityEngine;

namespace BeyondPixels.ECS.Components.Items
{
    public class ItemsStoreComponent : MonoBehaviour
    {
        public Item[] Items;

        [Serializable]
        public class Item
        {
            public Sprite[] Icons;
            public ItemType ItemType;
            public string Name;
            public ModifierType ModifierType;
            public int ModifierValue;
            public GearType GearType;
        }

    }
    public enum ItemType
    {
        Food = 1,
        Potion = 2,
        Gear = 3,
        Treasure = 4
    }

    public enum ModifierType
    {
        Health = 1,
        WeaponDamage = 2,
        MagicDamage = 3,
        Armor = 4
    }

    public enum GearType
    {
        Helmet = 1,
        Chest = 2,
        Weapon = 3,
        Boots = 4,
        Magic = 5
    }
}
