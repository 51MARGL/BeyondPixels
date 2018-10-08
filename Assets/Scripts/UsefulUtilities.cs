using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    class UsefulUtilities
    {
        /// <summary>
        ///     Moore neighborhood algorithm
        /// </summary>
        /// <param name="gridX"></param>
        /// <param name="gridY"></param>
        /// <returns></returns>
        public static int GetSurroundingWallCount(int gridX, int gridY, bool[,] map)
        {
            var wallCount = 0;
            for (var neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
            for (var neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
                if (IsOnBoard(neighbourX, neighbourY, map))
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                        wallCount += map[neighbourX, neighbourY] ? 0 : 1;
                }
                else
                {
                    wallCount++;
                }

            return wallCount;
        }


        /// <summary>
        ///     Checks borders of map/array
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool IsOnBoard(int x, int y, bool[,] map)
        {
            var width = map.GetLength(0);
            var height = map.GetLength(1);
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        /// <summary>
        ///     Checks if inside map boundaries
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool NotOnBorder(int x, int y, bool[,] map)
        {
            var width = map.GetLength(0);
            var height = map.GetLength(1);
            return y != 0 && x != 0 && y != height - 1 && x != width - 1;
        }
    }
}
