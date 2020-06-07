using BeyondPixels.ECS.Components.ProceduralGeneration.Dungeon.BSP;

using Unity.Collections;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Systems.ProceduralGeneration.Dungeon.BSP
{
    public class BSPTree
    {
        public BSPNode Root;
        public int Height;

        public BSPTree(int width, int height, int minRoomSize, ref Random random)
        {
            BSPNode.MinRoomSize = minRoomSize;
            this.Root = new BSPNode(new NodeComponent
            {
                RectBounds = new int4(0, 0, height, width)
            });
            this.Root.SplitNode(ref random);
            this.Height = this.Root.TreeHeight();
        }

        public NativeArray<NodeComponent> ToNativeArray()
        {
            var nodesArray = new NativeArray<NodeComponent>((int)math.pow(2, this.Height) - 1, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            for (var i = 0; i < nodesArray.Length; i++)
            {
                nodesArray[i] = new NodeComponent { IsNull = 1 };
            }

            this.MapToArray(this.Root, nodesArray, 0);
            return nodesArray;
        }

        private void MapToArray(BSPNode node, NativeArray<NodeComponent> array, int index)
        {
            if (node == null)
            {
                return;
            }

            array[index] = node.NodeComponent;
            if (node.LeftChild != null)
            {
                this.MapToArray(node.LeftChild, array, 2 * index + 1);
            }

            if (node.RightChild != null)
            {
                this.MapToArray(node.RightChild, array, 2 * index + 2);
            }
        }

        public class BSPNode
        {
            public BSPNode LeftChild;
            public BSPNode RightChild;
            public NodeComponent NodeComponent;
            public static int MinRoomSize;

            public BSPNode(NodeComponent nodeComponent)
            {
                this.NodeComponent = nodeComponent;
            }

            public int TreeHeight()
            {
                return 1 + math.max(this.LeftChild?.TreeHeight() ?? 0,
               this.RightChild?.TreeHeight() ?? 0);
            }

            public void SplitNode(ref Random random)
            {
                if (this.NodeComponent.RectBounds.z / 2 < MinRoomSize
                    && this.NodeComponent.RectBounds.w / 2 < MinRoomSize)
                {
                    this.NodeComponent.IsLeaf = 1;
                    return;
                }

                var rectBounds = this.NodeComponent.RectBounds;

                bool splitHorizontal;
                if (rectBounds.w / (float)rectBounds.z > 1)
                {
                    splitHorizontal = false;
                }
                else if (rectBounds.z / (float)rectBounds.w >= 1)
                {
                    splitHorizontal = true;
                }
                else
                {
                    splitHorizontal = random.NextBool();
                }

                if (splitHorizontal)
                {
                    var splitPosition = random.NextInt(MinRoomSize, rectBounds.z - MinRoomSize + 1);

                    this.LeftChild = new BSPNode(new NodeComponent
                    {
                        RectBounds = new int4(rectBounds.x, rectBounds.y, splitPosition, rectBounds.w)
                    });
                    this.RightChild = new BSPNode(new NodeComponent
                    {
                        RectBounds = new int4(rectBounds.x, rectBounds.y + splitPosition, rectBounds.z - splitPosition, rectBounds.w)
                    });
                }
                else
                {
                    var splitPosition = random.NextInt(MinRoomSize, rectBounds.w - MinRoomSize + 1);

                    this.LeftChild = new BSPNode(new NodeComponent
                    {
                        RectBounds = new int4(rectBounds.x, rectBounds.y, rectBounds.z, splitPosition)
                    });
                    this.RightChild = new BSPNode(new NodeComponent
                    {
                        RectBounds = new int4(rectBounds.x + splitPosition, rectBounds.y, rectBounds.z, rectBounds.w - splitPosition)
                    });
                }

                //Parallel.Invoke(
                //    () => this.LeftChild.SplitNode(ref random),
                //    () => this.RightChild.SplitNode(ref random),
                //);
                this.LeftChild.SplitNode(ref random);
                this.RightChild.SplitNode(ref random);
            }
        }
    }
}
