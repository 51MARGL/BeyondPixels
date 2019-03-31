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
using UnityEngine.SceneManagement;

namespace BeyondPixels.SceneBootstraps
{
    public class DungeonBootstrap : MonoBehaviour
    {
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
            Random, Naive, CellularAutomaton, BSP
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
            Entity board;
            switch (DungeonGenerators.Switch)
            {
                case Switch.Random:
                    var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.ToString("yyyyMMddHHmmssff").GetHashCode());
                    var randomAlg = random.NextInt(0, 100);
                    if (randomAlg < 33)
                    {
                        var randomSize = new int2(random.NextInt(100, 200), random.NextInt(50, 175));
                        var roomCount = randomSize.x * randomSize.y / 100;
                        board = entityManager.CreateEntity();
                        entityManager.AddComponentData(board, new ECS.Components.ProceduralGeneration.Dungeon.Naive.BoardComponent
                        {
                            Size = randomSize,
                            RoomCount = roomCount,
                            MaxRoomSize = DungeonGenerators.Naive.MaxRoomSize,
                            MaxCorridorLength = DungeonGenerators.Naive.MaxCorridorLength,
                            MinCorridorLength = DungeonGenerators.Naive.MinCorridorLength
                        });
                    }
                    else if (randomAlg < 66)
                    {
                        var randomSize = new int2(random.NextInt(100, 200), random.NextInt(50, 175));
                        var randomFillPercent = random.NextInt(60, 75);
                        board = entityManager.CreateEntity();
                        entityManager.AddComponentData(board, new ECS.Components.ProceduralGeneration.Dungeon.CellularAutomaton.BoardComponent
                        {
                            Size = new int2(DungeonGenerators.CellularAutomaton.BoardWidth, DungeonGenerators.CellularAutomaton.BoardHeight),
                            RandomFillPercent = randomFillPercent,
                            PassRadius = DungeonGenerators.CellularAutomaton.PassRadius
                        });
                        break;
                    }
                    else
                    {
                        var randomSize = new int2(random.NextInt(100, 200), random.NextInt(50, 175));
                        board = entityManager.CreateEntity();
                        entityManager.AddComponentData(board, new ECS.Components.ProceduralGeneration.Dungeon.BSP.BoardComponent
                        {
                            Size = randomSize,
                            MinRoomSize = DungeonGenerators.BSP.MinRoomSize
                        });
                        break;
                    }
                    break;
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
            #endregion


            var initPrefabManager = PrefabManager.Instance;
            var initSpellBookManager = SpellBookManagerComponent.Instance;
            #region UI
            UIManager.Instance.Initialize();
            #endregion
        }

        public void FixedUpdate()
        {
            FixedGroup.Update();
        }

        public void Update()
        {            
            if (Input.GetKeyDown(KeyCode.M))
                SceneManager.LoadScene("DungeonScene", LoadSceneMode.Single);
        }
    }
}
