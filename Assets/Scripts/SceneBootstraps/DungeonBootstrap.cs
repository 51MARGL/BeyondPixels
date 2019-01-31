﻿using BeyondPixels.Components.Characters.AI;
using BeyondPixels.Components.Characters.Common;
using BeyondPixels.Components.Characters.Player;
using BeyondPixels.Components.ProceduralGeneration.Dungeon.Naive;
using BeyondPixels.UI;
using Unity.Entities;
using UnityEngine;

namespace BeyondPixels.SceneBootstraps
{
    public class DungeonBootstrap : MonoBehaviour
    {
        public int BoardWidth;
        public int BoardHeight;
        public int RoomCount;
        public int RoomSize;
        public int MaxCorridorLength;
        public int MinCorridorLength;

        // Use this for initialization
        private void Start()
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();

            #region PlayerEntityArchetype
            var player = PrefabManager.Instance.PlayerPrefab;
            var playerEntity = player.GetComponent<GameObjectEntity>().Entity;
            var playerInitializeComponent = player.GetComponent<PlayerInitializeComponent>();
            entityManager.AddComponent(playerEntity, typeof(InputComponent));

            entityManager.AddComponentData(playerEntity, new CharacterComponent
            {
                CharacterType = CharacterType.Player
            });
            entityManager.AddComponentData(playerEntity, new MovementComponent
            {
                Direction = Vector2.zero,
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
                InitialPosition = player.transform.position
            });
            GameObject.Destroy(playerInitializeComponent);
            entityManager.RemoveComponent<PlayerInitializeComponent>(playerEntity);
            #endregion

            #region EnemyEntityArchetype
            //var enemy = PrefabManager.Instance.EnemyPrefab;
            var enemy = GameObject.Instantiate(PrefabManager.Instance.EnemyPrefab, new Vector3(-2, -2, 0), Quaternion.identity);
            var enemyEntity = enemy.GetComponent<GameObjectEntity>().Entity;
            var enemyInitializeComponent = enemy.GetComponent<EnemyInitializeComponent>();

            entityManager.AddComponentData(enemyEntity, new CharacterComponent
            {
                CharacterType = CharacterType.Enemy
            });
            entityManager.AddComponentData(enemyEntity, new MovementComponent
            {
                Direction = Vector2.zero,
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
                InitialPosition = enemy.transform.position
            });
            GameObject.Destroy(enemyInitializeComponent);
            entityManager.RemoveComponent<EnemyInitializeComponent>(enemyEntity);
            #endregion


            #region DungeonGeneration
            var board = entityManager.CreateEntity();
            entityManager.AddComponentData(board, new BoardComponent
            {
                Size = new Unity.Mathematics.int2(BoardWidth, BoardHeight),
                RoomCount = RoomCount,
                RoomSize = RoomSize,
                MaxCorridorLength = MaxCorridorLength,
                MinCorridorLength = MinCorridorLength
            });
            #endregion


            #region UI
            UIManager.Instance.Initialize(player);
            #endregion
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                var entityManager = World.Active.GetOrCreateManager<EntityManager>();
                for (int i = 0; i < 20; i++)
                {
                    var randomPositon = new Vector2(Random.Range(-50f, 50f), Random.Range(-50f, 50f));
                    var enemy = GameObject.Instantiate(PrefabManager.Instance.EnemyPrefab, randomPositon, Quaternion.identity);
                    var enemyEntity = enemy.GetComponent<GameObjectEntity>().Entity;
                    var enemyInitializeComponent = enemy.GetComponent<EnemyInitializeComponent>();

                    entityManager.AddComponentData(enemyEntity, new CharacterComponent
                    {
                        CharacterType = CharacterType.Enemy
                    });
                    entityManager.AddComponentData(enemyEntity, new MovementComponent
                    {
                        Direction = Vector2.zero,
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
                        InitialPosition = enemy.transform.position
                    });
                    GameObject.Destroy(enemyInitializeComponent);
                    entityManager.RemoveComponent<EnemyInitializeComponent>(enemyEntity);
                }
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                var entityManager = World.Active.GetOrCreateManager<EntityManager>();

                var board = entityManager.CreateEntity();
                entityManager.AddComponentData(board, new BoardComponent
                {
                    Size = new Unity.Mathematics.int2(BoardWidth, BoardHeight),
                    RoomCount = RoomCount,
                    RoomSize = RoomSize,
                    MaxCorridorLength = MaxCorridorLength,
                    MinCorridorLength = MinCorridorLength
                });
            }
        }
    }
}