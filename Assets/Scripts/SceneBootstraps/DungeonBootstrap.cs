using System;
using BeyondPixels.UI;
using BeyondPixels.Utilities;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

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
            this.FixedGroup = World.Active.GetOrCreateSystem<FixedUpdateSystemGroup>();
            var entityManager = World.Active.EntityManager;

            #region DungeonGeneration
            Entity board;
            switch (this.DungeonGenerators.Switch)
            {
                case Switch.Random:
                    var random = new Unity.Mathematics.Random((uint)System.DateTime.Now.ToString("yyyyMMddHHmmssff").GetHashCode());
                    var randomAlg = random.NextInt(0, 100);
                    if (randomAlg < 33)
                    {
                        var randomSize = new int2(random.NextInt(100, 200), random.NextInt(50, 150));
                        var roomCount = (int)math.log2(randomSize.x * randomSize.y / 100) * random.NextInt(10, 20);
                        board = entityManager.CreateEntity();
                        entityManager.AddComponentData(board, new ECS.Components.ProceduralGeneration.Dungeon.Naive.BoardComponent
                        {
                            Size = randomSize,
                            RoomCount = roomCount,
                            MaxRoomSize = this.DungeonGenerators.Naive.MaxRoomSize,
                            MaxCorridorLength = this.DungeonGenerators.Naive.MaxCorridorLength,
                            MinCorridorLength = this.DungeonGenerators.Naive.MinCorridorLength
                        });
                    }
                    else if (randomAlg < 66)
                    {
                        var randomSize = new int2(random.NextInt(100, 200), random.NextInt(50, 150));
                        var randomFillPercent = random.NextInt(60, 75);
                        board = entityManager.CreateEntity();
                        entityManager.AddComponentData(board, new ECS.Components.ProceduralGeneration.Dungeon.CellularAutomaton.BoardComponent
                        {
                            Size = new int2(this.DungeonGenerators.CellularAutomaton.BoardWidth, this.DungeonGenerators.CellularAutomaton.BoardHeight),
                            RandomFillPercent = randomFillPercent,
                            PassRadius = this.DungeonGenerators.CellularAutomaton.PassRadius
                        });
                        break;
                    }
                    else
                    {
                        var randomSize = new int2(random.NextInt(100, 200), random.NextInt(50, 150));
                        board = entityManager.CreateEntity();
                        entityManager.AddComponentData(board, new ECS.Components.ProceduralGeneration.Dungeon.BSP.BoardComponent
                        {
                            Size = randomSize,
                            MinRoomSize = this.DungeonGenerators.BSP.MinRoomSize
                        });
                        break;
                    }
                    break;
                case Switch.Naive:
                    board = entityManager.CreateEntity();
                    entityManager.AddComponentData(board, new ECS.Components.ProceduralGeneration.Dungeon.Naive.BoardComponent
                    {
                        Size = new int2(this.DungeonGenerators.Naive.BoardWidth, this.DungeonGenerators.Naive.BoardHeight),
                        RoomCount = this.DungeonGenerators.Naive.RoomCount,
                        MaxRoomSize = this.DungeonGenerators.Naive.MaxRoomSize,
                        MaxCorridorLength = this.DungeonGenerators.Naive.MaxCorridorLength,
                        MinCorridorLength = this.DungeonGenerators.Naive.MinCorridorLength
                    });
                    break;
                case Switch.CellularAutomaton:
                    board = entityManager.CreateEntity();
                    entityManager.AddComponentData(board, new ECS.Components.ProceduralGeneration.Dungeon.CellularAutomaton.BoardComponent
                    {
                        Size = new int2(this.DungeonGenerators.CellularAutomaton.BoardWidth, this.DungeonGenerators.CellularAutomaton.BoardHeight),
                        RandomFillPercent = this.DungeonGenerators.CellularAutomaton.RandomFillPercent,
                        PassRadius = this.DungeonGenerators.CellularAutomaton.PassRadius
                    });
                    break;
                case Switch.BSP:
                    board = entityManager.CreateEntity();
                    entityManager.AddComponentData(board, new ECS.Components.ProceduralGeneration.Dungeon.BSP.BoardComponent
                    {
                        Size = new int2(this.DungeonGenerators.BSP.BoardWidth, this.DungeonGenerators.BSP.BoardHeight),
                        MinRoomSize = this.DungeonGenerators.BSP.MinRoomSize
                    });
                    break;
            }
            #endregion

            UIManager.Instance.MainMenu.InGameMenu = true;
        }

        public void FixedUpdate()
        {
            this.FixedGroup.Update();
        }
    }
}
