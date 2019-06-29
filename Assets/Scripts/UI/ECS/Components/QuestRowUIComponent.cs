using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.UI.Buttons;
using BeyondPixels.UI.Menus;
using TMPro;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.UI.ECS.Components
{
    public class QuestRowUIComponent : MonoBehaviour
    {
        public Entity QuestEntity;
        public TextMeshProUGUI QuestText;
        public TextMeshProUGUI QuestProgress;
        public SubmitButton CollectButton;

        public void Start()
        {
            this.CollectButton.OnSubmitEvent += () =>
            {
                var entityManager = World.Active.EntityManager;
                entityManager.AddComponentData(this.QuestEntity, new CollectXPRewardComponent());
                entityManager.AddComponentData(this.QuestEntity, new DestroyComponent());
            };
        }
    }
}
