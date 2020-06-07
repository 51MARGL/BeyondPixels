using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Objects;
using BeyondPixels.ECS.Components.Quest;
using BeyondPixels.UI.Buttons;

using System;
using System.Text;

using TMPro;

using Unity.Entities;

using UnityEngine;
using UnityEngine.EventSystems;

namespace BeyondPixels.UI.ECS.Components
{
    public class QuestRowUIComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Entity QuestEntity;
        public TextMeshProUGUI QuestText;
        public TextMeshProUGUI QuestProgress;
        public SubmitButton CollectButton;

        public void OnPointerEnter(PointerEventData eventData)
        {
            var entityManager = World.Active.EntityManager;
            var xpReward = entityManager.GetComponentData<XPRewardComponent>(this.QuestEntity);
            var lvlComp = entityManager.GetComponentData<LevelComponent>(this.QuestEntity);
            var header = "Quest";
            var sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            sb.Append(this.QuestText.text);
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append($"Progress: {this.QuestProgress.text}");
            sb.Append(Environment.NewLine);
            sb.Append($"Reward: {xpReward.XPAmount * lvlComp.CurrentLevel} XP");

            UIManager.Instance.ShowTooltip(this.transform.position, header, sb.ToString(), string.Empty);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UIManager.Instance.HideTooltip();
        }

        public void Start()
        {
            this.CollectButton.OnSubmitEvent += () =>
            {
                var entityManager = World.Active.EntityManager;
                entityManager.AddComponentData(this.QuestEntity, new CollectXPRewardComponent());
                entityManager.AddComponentData(this.QuestEntity, new DestroyComponent());

                var genQuestEntity = entityManager.CreateEntity();
                entityManager.AddComponentData(genQuestEntity, new GenerateQuestComponent());
            };
        }
    }
}
