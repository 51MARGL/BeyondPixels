using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    private int enemiesCount;
    private int enemiesSpawned;

    public List<GameObject> enemyPrefabs;

    private IEnumerable<MapTile> freeTilesList;
    private Player Player;

    public MapProvider MapProvider { get; set; }

    // Use this for initialization
    private void Start()
    {
        enemiesSpawned = 0;

        Player = FindObjectOfType<Player>();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void SpawnObjects()
    {
        //Spawn only on cells without walls near by 
        freeTilesList = MapProvider.GetFreeTiles()
            .Where(tile => UsefulUtilities.GetSurroundingWallCount(tile.X, tile.Y, MapProvider.Map) == 0);
        enemiesCount = freeTilesList.Count() / 100 * 2;
        Debug.Log("Enemies-Count:" + enemiesCount);
        MovePlayer();
        SpawnEnemies();
    }

    private void MovePlayer()
    {
        var mostLeft = freeTilesList.First();
        foreach (var freeTile in freeTilesList)
            if (freeTile.X < mostLeft.X && freeTile.Y < mostLeft.Y)
                mostLeft = freeTile;
        Player.transform.position = new Vector2(mostLeft.X, mostLeft.Y);
    }

    private void SpawnEnemies()
    {
        while (enemiesSpawned < enemiesCount && enemiesSpawned < 80)
        {
            var randomTile = freeTilesList.ElementAt(Random.Range(0, freeTilesList.Count()));
            if (Vector2.Distance(Player.transform.position, new Vector2(randomTile.X, randomTile.Y)) > 10)
            {
                var randomPrefab = enemyPrefabs.ElementAt(0);
                Instantiate(randomPrefab, new Vector2(randomTile.X, randomTile.Y), Quaternion.identity, transform);
                enemiesSpawned++;
            }
        }
    }
}