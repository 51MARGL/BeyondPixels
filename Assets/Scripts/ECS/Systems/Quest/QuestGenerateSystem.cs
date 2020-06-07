using BeyondPixels.ECS.Components.Characters.Level;
using BeyondPixels.ECS.Components.Items;
using BeyondPixels.ECS.Components.Quest;
using BeyondPixels.SceneBootstraps;

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace BeyondPixels.ECS.Systems.Quest
{
    public class QuestGenerateSystem : ComponentSystem
    {
        private Grammar Grammar;
        private EntityQuery _generateGroup;
        private EntityQuery _activeGroup;
        private string[] _quests;
        private Unity.Mathematics.Random _random;

        protected override void OnCreate()
        {
            this._random = new Unity.Mathematics.Random((uint)System.Guid.NewGuid().GetHashCode());

            if (this.Grammar == null)
            {
                this._quests = new[] {
                    "{Kill}", "{Find}", "{Release}",
                    "{Loot}", "{LevelUp}", "{SpendSkillPoint}"
                };

                this.Grammar = new Grammar
                {
                    TerminalAlphabet = new[] {
                        "{Quest}", "{Kill}", "{Find}",
                        "{Release}", "{Loot}", "{Item}",
                        "{LevelUp}", "{SpendSkillPoint}", "{1-15}", "{1-5}"
                    },
                    StartTerminal = "{Quest}",
                    Rules = new[]
                    {
                        new Rule
                        {
                            LeftSide = "{Quest}",
                            Products = this._quests
                        },
                        new Rule
                        {
                            LeftSide = "{1-15}",
                            Products = new [] { "[1-15]" }
                        },
                        new Rule
                        {
                            LeftSide = "{1-5}",
                            Products = new [] { "[1-5]" }
                        },
                        new Rule
                        {
                            LeftSide = "{Item}",
                            Products = new [] { "[food] item", "[potion]", "[gear] item" }
                        },
                        new Rule
                        {
                            LeftSide = "{Kill}",
                            Products = new [] { "[Defeat] {1-15} [ghost]" }
                        },
                        new Rule
                        {
                            LeftSide = "{Find}",
                            Products = new [] { "[Pick up] {1-15} {Item}" }
                        },
                        new Rule
                        {
                            LeftSide = "{Release}",
                            Products = new [] { "[Release] {1-15} [chicken]" }
                        },
                        new Rule
                        {
                            LeftSide = "{Loot}",
                            Products = new [] { "[Loot] {1-15} [chest]" }
                        },
                        new Rule
                        {
                            LeftSide = "{LevelUp}",
                            Products = new [] { "[Level up] {1-5} [time]" }
                        },
                        new Rule
                        {
                            LeftSide = "{SpendSkillPoint}",
                            Products = new [] { "[Spend] {1-5} [skill point]" }
                        }
                    }
                };
            }

            this._generateGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(GenerateQuestComponent)
                }
            });
            this._activeGroup = this.GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(QuestComponent)
                }
            });
        }

        protected override void OnUpdate()
        {
            if (this._generateGroup.CalculateEntityCount() == 0)
            {
                return;
            }

            var allowedArray = this._quests.ToList();

            this.Entities.With(this._activeGroup).ForEach((Entity activeEntity) =>
            {
                if (this.EntityManager.HasComponent<DefeatQuestComponent>(activeEntity))
                {
                    allowedArray.Remove("{Kill}");
                }

                if (this.EntityManager.HasComponent<PickUpQuestComponent>(activeEntity))
                {
                    allowedArray.Remove("{Find}");
                }

                if (this.EntityManager.HasComponent<ReleaseQuestComponent>(activeEntity))
                {
                    allowedArray.Remove("{Release}");
                }

                if (this.EntityManager.HasComponent<LootQuestComponent>(activeEntity))
                {
                    allowedArray.Remove("{Loot}");
                }

                if (this.EntityManager.HasComponent<LevelUpQuestComponent>(activeEntity))
                {
                    allowedArray.Remove("{LevelUp}");
                }

                if (this.EntityManager.HasComponent<SpendSkillPointQuestComponent>(activeEntity))
                {
                    allowedArray.Remove("{SpendSkillPoint}");
                }
            });

            var entities = this._generateGroup.ToEntityArray(Allocator.TempJob);
            for (var i = 0; i < entities.Length; i++)
            {
                var questText = string.Empty;
                if (allowedArray.Count == this._quests.Length)
                {
                    questText = this.Grammar.GenerateRandomText();

                }
                else if (allowedArray.Count > 0)
                {
                    questText = this.Grammar.GenerateRandomText(allowedArray[this._random.NextInt(0, allowedArray.Count)]);
                }

                var quest = this.CreateQuestEntity(ref questText, allowedArray);
                quest.GetComponent<QuestTextComponent>().QuestText = questText;

                this.PostUpdateCommands.DestroyEntity(entities[i]);
            }
            entities.Dispose();
        }

        private GameObject CreateQuestEntity(ref string template, List<string> allowedArray)
        {
            var questObj = GameObject.Instantiate(PrefabManager.Instance.QuestPrefab);
            var questEntity = questObj.GetComponent<GameObjectEntity>().Entity;
            var questType = Regex.Match(template, @"\[[^\[\]]+\]").Value;
            template = template.Replace(questType, questType.Substring(1, questType.Length - 2));
            switch (questType)
            {
                case "[Defeat]":
                    allowedArray.Remove("{Kill}");
                    this.InitDefeatQuest(ref template, questEntity);
                    break;
                case "[Pick up]":
                    allowedArray.Remove("{Find}");
                    this.InitPickUpQuest(ref template, questEntity);
                    break;
                case "[Release]":
                    allowedArray.Remove("{Release}");
                    this.InitReleaseQuest(ref template, questEntity);
                    break;
                case "[Loot]":
                    allowedArray.Remove("{Loot}");
                    this.InitLootQuest(ref template, questEntity);
                    break;
                case "[Level up]":
                    allowedArray.Remove("{LevelUp}");
                    this.InitLevelUpQuest(ref template, questEntity);
                    break;
                case "[Spend]":
                    allowedArray.Remove("{SpendSkillPoint}");
                    this.InitSpendPointsQuest(ref template, questEntity);
                    break;
            }

            this.PostUpdateCommands.AddComponent(questEntity, new LevelComponent
            {
                CurrentLevel = 1
            });
            return questObj;
        }

        private void InitDefeatQuest(ref string template, Entity questEntity)
        {
            this.PostUpdateCommands.AddComponent(questEntity, new DefeatQuestComponent());
            var prTarget = this.AddProgressTarget(questEntity, ref template);
            if (prTarget > 1)
            {
                template += "s";
            }

            this.PostUpdateCommands.AddComponent(questEntity, new XPRewardComponent
            {
                XPAmount = 7 * prTarget
            });

            var target = Regex.Match(template, @"\[[^\[\]]+\]").Value;
            template = template.Replace(target, target.Substring(1, target.Length - 2));
        }

        private void InitPickUpQuest(ref string template, Entity questEntity)
        {
            var prTarget = this.AddProgressTarget(questEntity, ref template);
            if (prTarget > 1)
            {
                template += "s";
            }

            this.PostUpdateCommands.AddComponent(questEntity, new XPRewardComponent
            {
                XPAmount = 5 * prTarget
            });

            var target = Regex.Match(template, @"\[[^\[\]]+\]").Value;
            template = template.Replace(target, target.Substring(1, target.Length - 2));

            if (System.Enum.TryParse<ItemType>(target.Substring(1, target.Length - 2), true, out var itemType))
            {
                this.PostUpdateCommands.AddComponent(questEntity, new PickUpQuestComponent
                {
                    ItemType = itemType
                });
            }
        }

        private void InitReleaseQuest(ref string template, Entity questEntity)
        {
            this.PostUpdateCommands.AddComponent(questEntity, new ReleaseQuestComponent());
            var prTarget = this.AddProgressTarget(questEntity, ref template);
            if (prTarget > 1)
            {
                template += "s";
            }

            this.PostUpdateCommands.AddComponent(questEntity, new XPRewardComponent
            {
                XPAmount = 7 * prTarget
            });

            var target = Regex.Match(template, @"\[[^\[\]]+\]").Value;
            template = template.Replace(target, target.Substring(1, target.Length - 2));
        }

        private void InitLootQuest(ref string template, Entity questEntity)
        {
            this.PostUpdateCommands.AddComponent(questEntity, new LootQuestComponent());
            var prTarget = this.AddProgressTarget(questEntity, ref template);
            if (prTarget > 1)
            {
                template += "s";
            }

            this.PostUpdateCommands.AddComponent(questEntity, new XPRewardComponent
            {
                XPAmount = 5 * prTarget
            });

            var target = Regex.Match(template, @"\[[^\[\]]+\]").Value;
            template = template.Replace(target, target.Substring(1, target.Length - 2));
        }

        private void InitLevelUpQuest(ref string template, Entity questEntity)
        {
            this.PostUpdateCommands.AddComponent(questEntity, new LevelUpQuestComponent());
            var prTarget = this.AddProgressTarget(questEntity, ref template);
            if (prTarget > 1)
            {
                template += "s";
            }

            this.PostUpdateCommands.AddComponent(questEntity, new XPRewardComponent
            {
                XPAmount = 10 * prTarget
            });

            var target = Regex.Match(template, @"\[[^\[\]]+\]").Value;
            template = template.Replace(target, target.Substring(1, target.Length - 2));
        }

        private void InitSpendPointsQuest(ref string template, Entity questEntity)
        {
            this.PostUpdateCommands.AddComponent(questEntity, new SpendSkillPointQuestComponent());
            var prTarget = this.AddProgressTarget(questEntity, ref template);
            if (prTarget > 1)
            {
                template += "s";
            }

            this.PostUpdateCommands.AddComponent(questEntity, new XPRewardComponent
            {
                XPAmount = 5 * prTarget
            });

            var target = Regex.Match(template, @"\[[^\[\]]+\]").Value;
            template = template.Replace(target, target.Substring(1, target.Length - 2));
        }

        private int AddProgressTarget(Entity questEntity, ref string template)
        {
            var progressTarget = 0;
            var progressMatch = Regex.Match(template, @"\[\d+\]")?.Value;
            if (!string.IsNullOrEmpty(progressMatch))
            {
                var num = progressMatch.Substring(1, progressMatch.Length - 2);
                progressTarget = int.Parse(num);
                template = template.Replace(progressMatch, num);
            }

            this.PostUpdateCommands.AddComponent(questEntity, new QuestComponent
            {
                CurrentProgress = 0,
                ProgressTarget = progressTarget
            });

            return progressTarget;
        }
    }
}