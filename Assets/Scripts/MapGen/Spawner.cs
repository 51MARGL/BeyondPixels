using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public List<GameObject> enemyPrefabs;
    private Player Player;
    private int enemiesCount;
    private int enemiesSpawned;
    GenCave generator;

    private List<GenCave.TCoord> freeTiles;

    // Use this for initialization
    void Start()
    {
        enemiesSpawned = 0;
        generator = FindObjectOfType<GenCave>();
        Player = FindObjectOfType<Player>();
        generator.boardIsReady += SpawnObjects;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SpawnObjects()
    {
        freeTiles = generator.FreeTilesList;
        enemiesCount = freeTiles.Count / 100 * 2;
        Debug.Log("Enemies-Count:" + enemiesCount);
        MovePlayer();
        SpawnEnemies();
    }

    void MovePlayer()
    {
        GenCave.TCoord mostLeft = freeTiles[0];
        foreach (var freeTile in freeTiles)
        {
            if (freeTile.x < mostLeft.x && freeTile.y < mostLeft.y)
                mostLeft = freeTile;
        }
        Player.transform.position = new Vector2(mostLeft.x, mostLeft.y);        
        return;
    }

    void SpawnEnemies()
    {
        while (enemiesSpawned < enemiesCount)
        {
            var randomTile = freeTiles[Random.Range(0, freeTiles.Count)];
            if (Vector2.Distance(Player.transform.position, new Vector2(randomTile.x, randomTile.y)) > 10)
            {
                var randomPrefab = enemyPrefabs.ElementAt(0);
                Instantiate(randomPrefab, new Vector2(randomTile.x, randomTile.y), Quaternion.identity, transform);
                enemiesSpawned++;
            }
        }
    }
}
