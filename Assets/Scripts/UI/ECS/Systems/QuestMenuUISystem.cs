using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Game;
using BeyondPixels.ECS.Components.Quest;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.UI.ECS.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class QuestMenuUISystem : ComponentSystem
    {
        private EntityQuery _activeGroup;
        private EntityQuery _playerGroup;

        protected override void OnCreate()
        {
            this._activeGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(QuestComponent),
                    typeof(QuestTextComponent)
                }
            });

            this._playerGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                    typeof(PlayerComponent),
                },
                None = new ComponentType[] {
                    typeof(InCutsceneComponent),
                }
            });
        }

        protected override void OnUpdate()
        {
            if (this._playerGroup.CalculateLength() == 0)
            {
                UIManager.Instance.QuestMenu.Hide();
                return;
            }

            if (Input.GetKeyDown(SettingsManager.Instance.GetKeyBindValue(KeyBindName.Quest))
                    && Time.timeScale > 0)
            {
                if (UIManager.Instance.QuestMenu.IsVisible)
                {
                    UIManager.Instance.QuestMenu.Hide();
                }
                else
                {
                    UIManager.Instance.LootBagMenuUIComponent.Hide();
                    UIManager.Instance.PlayerInfoMenuUIComponent.Hide();

                    UIManager.Instance.QuestMenu.Show();
                }
            }

            if (UIManager.Instance.QuestMenu.IsVisible)
            {
                var index = 0;
                this.Entities.With(this._activeGroup).ForEach((Entity questEntity, 
                    QuestTextComponent questTextComponent, ref QuestComponent questComponent) =>
                {
                    var row = UIManager.Instance.QuestMenu.QuestRows[index];
                    row.gameObject.SetActive(true);
                    row.QuestEntity = questEntity;
                    row.QuestText.text = questTextComponent.QuestText;
                    row.QuestProgress.text = questComponent.CurrentProgress + "/" + questComponent.ProgressTarget;
                    row.CollectButton.gameObject.SetActive(
                        this.EntityManager.HasComponent<QuestDoneComponent>(questEntity));
                    index++;
                });

                for (int i = index; i < UIManager.Instance.QuestMenu.QuestRows.Length; i++)
                    UIManager.Instance.QuestMenu.QuestRows[i].gameObject.SetActive(false);
            }
        }
    }
}
