using System;
using System.Collections.Generic;

public interface MapProvider
{
    bool[,] Map { get; set; }

    int Width { get; set; }

    int Height { get; set; }

    /// <summary>
    ///     Action called when map is ready
    /// </summary>
    event Action MapIsReady;

    /// <summary>
    ///     Starts to generate map and checks map consistency
    /// </summary>
    void GenerateMap();

    /// <summary>
    ///     Counts amount of live cells
    /// </summary>
    /// <returns>List of live cells</returns>
    List<MapTile> GetFreeTiles();
}