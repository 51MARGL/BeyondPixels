using System;
using BeyondPixels.UI.Buttons;
using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace BeyondPixels.UI.ECS.Components
{
    public class LootBagMenuUIComponent : MonoBehaviour
    {
        public LootGroupWrapper LootGroup;

        [Serializable]
        public class LootGroupWrapper
        {
            public GameObject Grid;
            public GameObject ItemButtonPrefab;
        }
    }
}
