using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    private int enemiesCount;
    private int enemiesSpawned;

    public List<GameObject> enemyPrefabs;

    private List<MapTile> freeTilesList;
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
        freeTilesList = MapProvider.GetFreeTiles();
        enemiesCount = freeTilesList.Count / 100 * 2;
        Debug.Log("Enemies-Count:" + enemiesCount);
        MovePlayer();
        SpawnEnemies();
    }

    private void MovePlayer()
    {
        MapTile mostLeft = freeTilesList[0];
        foreach (var freeTile in freeTilesList)
            if (freeTile.X < mostLeft.X && freeTile.Y < mostLeft.Y)
                mostLeft = freeTile;
        Player.transform.position = new Vector2(mostLeft.X, mostLeft.Y);
    }

    private void SpawnEnemies()
    {
        while (enemiesSpawned < enemiesCount && enemiesSpawned < 80)
        {
            var randomTile = freeTilesList[Random.Range(0, freeTilesList.Count)];
            if (Vector2.Distance(Player.transform.position, new Vector2(randomTile.X, randomTile.Y)) > 10)
            {
                var randomPrefab = enemyPrefabs.ElementAt(0);
                Instantiate(randomPrefab, new Vector2(randomTile.X, randomTile.Y), Quaternion.identity, transform);
                enemiesSpawned++;
            }
        }
    }
}