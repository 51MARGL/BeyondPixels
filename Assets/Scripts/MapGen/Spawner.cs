using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public List<GameObject> enemyPrefabs;
    public GameObject Player;
    private int enemiesCount;
    private int enemiesSpawned;
    GenCave generator;

    private List<GenCave.TCoord> freeTiles;

    // Use this for initialization
    void Start()
    {
        enemiesSpawned = 0;
        generator = FindObjectOfType<GenCave>();
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
        SpawnPlayer();
        SpawnEnemies();
    }

    void SpawnPlayer()
    {
        GenCave.TCoord mostLeft = freeTiles[0];
        foreach (var freeTile in freeTiles)
        {
            if (freeTile.x < mostLeft.x && freeTile.y < mostLeft.y)
                mostLeft = freeTile;
        }
        Instantiate(Player, new Vector2(mostLeft.x, mostLeft.y), Quaternion.identity, transform);
        GameManager.Player = FindObjectOfType<Player>();
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
