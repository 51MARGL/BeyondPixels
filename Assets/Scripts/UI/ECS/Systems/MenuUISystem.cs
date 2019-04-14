using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Characters.Stats;
using BeyondPixels.ECS.Components.Spells;
using BeyondPixels.UI.ECS.Components;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.UI.ECS.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class MenuUISystem : ComponentSystem
    {
        private ComponentGroup _playerGroup;
        private ComponentGroup _addStatButtonEventsGroup;

        protected override void OnCreateManager()
        {
            _playerGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(PlayerComponent),
                    typeof(HealthStatComponent),typeof(AttackStatComponent),
                    typeof(DefenceStatComponent), typeof(MagicStatComponent), typeof(LevelComponent)
                }
            });
            _addStatButtonEventsGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[] {
                    typeof(AddStatButtonPressedComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.deltaTime;
            var uiComponent = UIManager.Instance.GameUIComponent;
            var spellBook = SpellBookManagerComponent.Instance.SpellBook;

            Entities.With(_playerGroup).ForEach((Entity playerEntity,
                ref HealthStatComponent healthstatComponent,
                ref AttackStatComponent attackStatComponent,
                ref DefenceStatComponent defenceStatComponent,
                ref MagicStatComponent magicStatComponent,
                ref LevelComponent levelComponent) =>
            {

                var infoMenuComponent = UIManager.Instance.PlayerInfoMenuUIComponent;
                if (Input.GetKeyDown(KeyCode.I))
                {
                    if (infoMenuComponent.GetComponent<CanvasGroup>().alpha == 1)
                        infoMenuComponent.GetComponent<CanvasGroup>().alpha = 0;
                    else
                        infoMenuComponent.GetComponent<CanvasGroup>().alpha = 1;
                }


                #region playerInfoMenu
                if (infoMenuComponent.GetComponent<CanvasGroup>().alpha == 1)
                {
                    infoMenuComponent.LevelGroup.Level.text = levelComponent.CurrentLevel.ToString();
                    infoMenuComponent.LevelGroup.SkillPoints.text = levelComponent.SkillPoints.ToString();

                    var addPointButtonAlpha = levelComponent.SkillPoints > 0 ? 1 : 0;

                    infoMenuComponent.StatsGroup.HealthStat.PointsSpent.text = healthstatComponent.PointsSpent.ToString();
                    infoMenuComponent.StatsGroup.HealthStat.AddButton.GetComponent<CanvasGroup>().alpha = addPointButtonAlpha;

                    infoMenuComponent.StatsGroup.AttackStat.PointsSpent.text = attackStatComponent.PointsSpent.ToString();
                    infoMenuComponent.StatsGroup.AttackStat.AddButton.GetComponent<CanvasGroup>().alpha = addPointButtonAlpha;

                    infoMenuComponent.StatsGroup.DefenceStat.PointsSpent.text = defenceStatComponent.PointsSpent.ToString();
                    infoMenuComponent.StatsGroup.DefenceStat.AddButton.GetComponent<CanvasGroup>().alpha = addPointButtonAlpha;

                    infoMenuComponent.StatsGroup.MagicStat.PointsSpent.text = magicStatComponent.PointsSpent.ToString();
                    infoMenuComponent.StatsGroup.MagicStat.AddButton.GetComponent<CanvasGroup>().alpha = addPointButtonAlpha;
                }

                var lvlComp = levelComponent;
                var hpComp = healthstatComponent;
                var aComp = attackStatComponent;
                var dComp = defenceStatComponent;
                var mComp = magicStatComponent;
                Entities.With(_addStatButtonEventsGroup).ForEach((Entity eventEntity, ref AddStatButtonPressedComponent eventComponent) =>
                {
                    if (lvlComp.SkillPoints > 0)
                    {
                        switch (eventComponent.StatTarget)
                        {
                            case StatTarget.HealthStat:
                                hpComp.PointsSpent++;
                                lvlComp.SkillPoints--;
                                break;
                            case StatTarget.AttackStat:
                                aComp.PointsSpent++;
                                lvlComp.SkillPoints--;
                                break;
                            case StatTarget.DefenceStat:
                                dComp.PointsSpent++;
                                lvlComp.SkillPoints--;
                                break;
                            case StatTarget.MagicStat:
                                mComp.PointsSpent++;
                                lvlComp.SkillPoints--;
                                break;
                        }
                        PostUpdateCommands.AddComponent(playerEntity, new AdjustStatsComponent());
                    }
                    PostUpdateCommands.DestroyEntity(eventEntity);
                });
                #endregion

                levelComponent = lvlComp;
                healthstatComponent = hpComp;
                attackStatComponent = aComp;
                defenceStatComponent = dComp;
                magicStatComponent = mComp;

            });
        }
    }
}
