using BeyondPixels.UI;
using BeyondPixels.Utilities;

using System;

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
            var random = new Unity.Mathematics.Random((uint)System.Guid.NewGuid().GetHashCode());
            var randomSeed = random.NextUInt(1, uint.MaxValue);
            switch (this.DungeonGenerators.Switch)
            {
                case Switch.Random:
                    var randomAlg = random.NextInt(0, 100);
                    if (randomAlg < 33)
                    {
                        var randomSize = new int2(random.NextInt(75, 150), random.NextInt(50, 150));
                        var roomCount = (int)(randomSize.x * randomSize.y * random.NextFloat(0.0025f, 0.004f));
                        board = entityManager.CreateEntity();
                        entityManager.AddComponentData(board, new ECS.Components.ProceduralGeneration.Dungeon.Naive.BoardComponent
                        {
                            Size = randomSize,
                            RoomCount = roomCount,
                            MaxRoomSize = this.DungeonGenerators.Naive.MaxRoomSize,
                            MaxCorridorLength = this.DungeonGenerators.Naive.MaxCorridorLength,
                            MinCorridorLength = this.DungeonGenerators.Naive.MinCorridorLength,
                            RandomSeed = randomSeed
                        });
                    }
                    else if (randomAlg < 66)
                    {
                        var randomSize = new int2(random.NextInt(75, 150), random.NextInt(50, 150));
                        var randomFillPercent = random.NextInt(60, 70);
                        board = entityManager.CreateEntity();
                        entityManager.AddComponentData(board, new ECS.Components.ProceduralGeneration.Dungeon.CellularAutomaton.BoardComponent
                        {
                            Size = randomSize,
                            RandomFillPercent = randomFillPercent,
                            PassRadius = this.DungeonGenerators.CellularAutomaton.PassRadius,
                            RandomSeed = randomSeed
                        });
                        break;
                    }
                    else
                    {
                        var randomSize = new int2(random.NextInt(75, 150), random.NextInt(50, 150));
                        board = entityManager.CreateEntity();
                        entityManager.AddComponentData(board, new ECS.Components.ProceduralGeneration.Dungeon.BSP.BoardComponent
                        {
                            Size = randomSize,
                            MinRoomSize = random.NextInt(7, 13),
                            RandomSeed = randomSeed
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
                        MinCorridorLength = this.DungeonGenerators.Naive.MinCorridorLength,
                        RandomSeed = randomSeed
                    });
                    break;
                case Switch.CellularAutomaton:
                    board = entityManager.CreateEntity();
                    entityManager.AddComponentData(board, new ECS.Components.ProceduralGeneration.Dungeon.CellularAutomaton.BoardComponent
                    {
                        Size = new int2(this.DungeonGenerators.CellularAutomaton.BoardWidth, this.DungeonGenerators.CellularAutomaton.BoardHeight),
                        RandomFillPercent = this.DungeonGenerators.CellularAutomaton.RandomFillPercent,
                        PassRadius = this.DungeonGenerators.CellularAutomaton.PassRadius,
                        RandomSeed = randomSeed
                    });
                    break;
                case Switch.BSP:
                    board = entityManager.CreateEntity();
                    entityManager.AddComponentData(board, new ECS.Components.ProceduralGeneration.Dungeon.BSP.BoardComponent
                    {
                        Size = new int2(this.DungeonGenerators.BSP.BoardWidth, this.DungeonGenerators.BSP.BoardHeight),
                        MinRoomSize = this.DungeonGenerators.BSP.MinRoomSize,
                        RandomSeed = randomSeed
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
