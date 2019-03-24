using System;
using BeyondPixels.ECS.Components.Characters.AI;
using BeyondPixels.ECS.Components.Characters.Common;
using BeyondPixels.ECS.Components.Characters.Player;
using BeyondPixels.ECS.Components.Spells;
using BeyondPixels.UI;
using BeyondPixels.Utilities;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BeyondPixels.SceneBootstraps
{
    public class DungeonBootstrap : MonoBehaviour
    {

        public static SpellBookComponent spellBook;
        [Serializable]
        public class DungeonGeneratorSettings
        {
            public Switch Switch;
            public NaiveSettings Naive;
            public CellularAutomatonSettings CellularAutomaton;
            public BSPSettings BSP;
        }

        public enum Switch
        {
            Naive, CellularAutomaton, BSP
        }

        [Serializable]
        public class NaiveSettings
        {
            public int BoardWidth;
            public int BoardHeight;
            public int RoomCount;
            public int MaxRoomSize;
            public int MaxCorridorLength;
            public int MinCorridorLength;
        }

        [Serializable]
        public class CellularAutomatonSettings
        {
            public int BoardWidth;
            public int BoardHeight;
            [Range(1, 100)]
            public int RandomFillPercent;
            public int PassRadius;
        }

        [Serializable]
        public class BSPSettings
        {
            public int BoardWidth;
            public int BoardHeight;
            public int MinRoomSize;
        }

        public DungeonGeneratorSettings DungeonGenerators;
        private FixedUpdateSystemGroup FixedGroup;

        // Use this for initialization
        private void Start()
        {
            FixedGroup = World.Active.GetOrCreateManager<FixedUpdateSystemGroup>();
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();

            #region DungeonGeneration
            //Entity board;
            //switch (DungeonGenerators.Switch)
            //{
            //    case Switch.Naive:
            //        board = entityManager.CreateEntity();
            //        entityManager.AddComponentData(board, new ECS.Components.ProceduralGeneration.Dungeon.Naive.BoardComponent
            //        {
            //            Size = new int2(DungeonGenerators.Naive.BoardWidth, DungeonGenerators.Naive.BoardHeight),
            //            RoomCount = DungeonGenerators.Naive.RoomCount,
            //            MaxRoomSize = DungeonGenerators.Naive.MaxRoomSize,
            //            MaxCorridorLength = DungeonGenerators.Naive.MaxCorridorLength,
            //            MinCorridorLength = DungeonGenerators.Naive.MinCorridorLength
            //        });
            //        break;
            //    case Switch.CellularAutomaton:
            //        board = entityManager.CreateEntity();
            //        entityManager.AddComponentData(board, new ECS.Components.ProceduralGeneration.Dungeon.CellularAutomaton.BoardComponent
            //        {
            //            Size = new int2(DungeonGenerators.CellularAutomaton.BoardWidth, DungeonGenerators.CellularAutomaton.BoardHeight),
            //            RandomFillPercent = DungeonGenerators.CellularAutomaton.RandomFillPercent,
            //            PassRadius = DungeonGenerators.CellularAutomaton.PassRadius
            //        });
            //        break;
            //    case Switch.BSP:
            //        board = entityManager.CreateEntity();
            //        entityManager.AddComponentData(board, new ECS.Components.ProceduralGeneration.Dungeon.BSP.BoardComponent
            //        {
            //            Size = new int2(DungeonGenerators.BSP.BoardWidth, DungeonGenerators.BSP.BoardHeight),
            //            MinRoomSize = DungeonGenerators.BSP.MinRoomSize
            //        });
            //        break;
            //}
            #endregion

            spellBook = GameObject.FindGameObjectWithTag("SpellBook").GetComponent<SpellBookComponent>();

            #region PlayerEntityArchetype
            var player = PrefabManager.Instance.PlayerPrefab;
            var playerEntity = player.GetComponent<GameObjectEntity>().Entity;
            var playerInitializeComponent = player.GetComponent<PlayerInitializeComponent>();
            entityManager.AddComponent(playerEntity, typeof(PlayerComponent));
            entityManager.AddComponent(playerEntity, typeof(InputComponent));

            entityManager.AddComponentData(playerEntity, new CharacterComponent
            {
                CharacterType = CharacterType.Player
            });
            entityManager.AddComponentData(playerEntity, new MovementComponent
            {
                Direction = float2.zero,
                Speed = playerInitializeComponent.MovementSpeed
            });
            entityManager.AddComponentData(playerEntity, new HealthComponent
            {
                MaxValue = playerInitializeComponent.MaxHealth,
                CurrentValue = playerInitializeComponent.MaxHealth
            });
            entityManager.AddComponentData(playerEntity, new WeaponComponent
            {
                DamageValue = playerInitializeComponent.WeaponDamage
            });
            entityManager.AddComponentData(playerEntity, new PositionComponent
            {
                InitialPosition = new float2(player.transform.position.x, player.transform.position.y)
            });
            GameObject.Destroy(playerInitializeComponent);
            entityManager.RemoveComponent<PlayerInitializeComponent>(playerEntity);
            #endregion            

            for (int i = 0; i < 3; i++)
            {
                var spellEntity = entityManager.CreateEntity(typeof(ActiveSpellComponent));
                entityManager.SetComponentData(spellEntity, new ActiveSpellComponent
                {
                    Owner = playerEntity,
                    ActionIndex = i + 1,
                    SpellIndex = i
                });
            }
            #region UI
            UIManager.Instance.Initialize(spellBook);
            #endregion

            var randomPositon = new Vector3(-1, -1, 0);
            var enemy = GameObject.Instantiate(PrefabManager.Instance.EnemyPrefab, randomPositon, Quaternion.identity);
            var enemyEntity = enemy.GetComponent<GameObjectEntity>().Entity;
            var enemyInitializeComponent = enemy.GetComponent<EnemyInitializeComponent>();

            entityManager.AddComponentData(enemyEntity, new CharacterComponent
            {
                CharacterType = CharacterType.Enemy
            });
            entityManager.AddComponentData(enemyEntity, new MovementComponent
            {
                Direction = float2.zero,
                Speed = enemyInitializeComponent.MovementSpeed
            });
            entityManager.AddComponentData(enemyEntity, new HealthComponent
            {
                MaxValue = enemyInitializeComponent.MaxHealth,
                CurrentValue = enemyInitializeComponent.MaxHealth
            });
            entityManager.AddComponentData(enemyEntity, new WeaponComponent
            {
                DamageValue = enemyInitializeComponent.WeaponDamage,
                AttackRange = enemyInitializeComponent.AttackRange,
                CoolDown = enemyInitializeComponent.AttackCoolDown
            });
            entityManager.AddComponentData(enemyEntity, new IdleStateComponent
            {
                StartedAt = Time.time
            });
            entityManager.AddComponentData(enemyEntity, new PositionComponent
            {
                InitialPosition = new float2(enemy.transform.position.x, enemy.transform.position.y)
            });
            GameObject.Destroy(enemyInitializeComponent);
            entityManager.RemoveComponent<EnemyInitializeComponent>(enemyEntity);
        }

        public void FixedUpdate()
        {
            FixedGroup.Update();
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                var entityManager = World.Active.GetOrCreateManager<EntityManager>();
                for (int i = 0; i < 20; i++)
                {
                    var randomPositon = new Vector2(UnityEngine.Random.Range(-25f, 25f), UnityEngine.Random.Range(-25f, 25f));
                    var enemy = GameObject.Instantiate(PrefabManager.Instance.EnemyPrefab, randomPositon, Quaternion.identity);
                    var enemyEntity = enemy.GetComponent<GameObjectEntity>().Entity;
                    var enemyInitializeComponent = enemy.GetComponent<EnemyInitializeComponent>();

                    entityManager.AddComponentData(enemyEntity, new CharacterComponent
                    {
                        CharacterType = CharacterType.Enemy
                    });
                    entityManager.AddComponentData(enemyEntity, new MovementComponent
                    {
                        Direction = float2.zero,
                        Speed = enemyInitializeComponent.MovementSpeed
                    });
                    entityManager.AddComponentData(enemyEntity, new HealthComponent
                    {
                        MaxValue = enemyInitializeComponent.MaxHealth,
                        CurrentValue = enemyInitializeComponent.MaxHealth
                    });
                    entityManager.AddComponentData(enemyEntity, new WeaponComponent
                    {
                        DamageValue = enemyInitializeComponent.WeaponDamage,
                        AttackRange = enemyInitializeComponent.AttackRange,
                        CoolDown = enemyInitializeComponent.AttackCoolDown
                    });
                    entityManager.AddComponentData(enemyEntity, new IdleStateComponent
                    {
                        StartedAt = Time.time
                    });
                    entityManager.AddComponentData(enemyEntity, new PositionComponent
                    {
                        InitialPosition = new float2(enemy.transform.position.x, enemy.transform.position.y)
                    });
                    GameObject.Destroy(enemyInitializeComponent);
                    entityManager.RemoveComponent<EnemyInitializeComponent>(enemyEntity);
                }
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                var entityManager = World.Active.GetOrCreateManager<EntityManager>();

                Entity board;
                switch (DungeonGenerators.Switch)
                {
                    case Switch.Naive:
                        board = entityManager.CreateEntity();
                        entityManager.AddComponentData(board, new ECS.Components.ProceduralGeneration.Dungeon.Naive.BoardComponent
                        {
                            Size = new int2(DungeonGenerators.Naive.BoardWidth, DungeonGenerators.Naive.BoardHeight),
                            RoomCount = DungeonGenerators.Naive.RoomCount,
                            MaxRoomSize = DungeonGenerators.Naive.MaxRoomSize,
                            MaxCorridorLength = DungeonGenerators.Naive.MaxCorridorLength,
                            MinCorridorLength = DungeonGenerators.Naive.MinCorridorLength
                        });
                        break;
                    case Switch.CellularAutomaton:
                        board = entityManager.CreateEntity();
                        entityManager.AddComponentData(board, new ECS.Components.ProceduralGeneration.Dungeon.CellularAutomaton.BoardComponent
                        {
                            Size = new int2(DungeonGenerators.CellularAutomaton.BoardWidth, DungeonGenerators.CellularAutomaton.BoardHeight),
                            RandomFillPercent = DungeonGenerators.CellularAutomaton.RandomFillPercent,
                            PassRadius = DungeonGenerators.CellularAutomaton.PassRadius
                        });
                        break;
                    case Switch.BSP:
                        board = entityManager.CreateEntity();
                        entityManager.AddComponentData(board, new ECS.Components.ProceduralGeneration.Dungeon.BSP.BoardComponent
                        {
                            Size = new int2(DungeonGenerators.BSP.BoardWidth, DungeonGenerators.BSP.BoardHeight),
                            MinRoomSize = DungeonGenerators.BSP.MinRoomSize
                        });
                        break;
                }
            }
        }
    }
}
