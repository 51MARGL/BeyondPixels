using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{

    public List<GameObject> enemyPrefabs;
    private Player Player;
    private int enemiesCount;
    private int enemiesSpawned;
    DungeonProvider generator;

    private List<DungeonProvider.Tcoord> freeTiles;

    // Use this for initialization
    void Start()
    {
        enemiesSpawned = 0;
        generator = FindObjectOfType<DungeonProvider>();
        Player = FindObjectOfType<Player>();
        generator.BoardIsReady += SpawnObjects;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void SpawnObjects()
    {
        freeTiles = generator.FreeTilesList;
        enemiesCount = freeTiles.Count / 100 * 2;
        Debug.Log("Enemies-Count:" + enemiesCount);
        MovePlayer();
        SpawnEnemies();
    }

    private void MovePlayer()
    {
        DungeonProvider.Tcoord mostLeft = freeTiles[0];
        foreach (var freeTile in freeTiles)
        {
            if (freeTile.X < mostLeft.X && freeTile.Y < mostLeft.Y)
                mostLeft = freeTile;
        }
        Player.transform.position = new Vector2(mostLeft.X, mostLeft.Y);        
        return;
    }

    private void SpawnEnemies()
    {
        while (enemiesSpawned < enemiesCount)
        {
            var randomTile = freeTiles[Random.Range(0, freeTiles.Count)];
            if (Vector2.Distance(Player.transform.position, new Vector2(randomTile.X, randomTile.Y)) > 10)
            {
                var randomPrefab = enemyPrefabs.ElementAt(0);
                Instantiate(randomPrefab, new Vector2(randomTile.X, randomTile.Y), Quaternion.identity, transform);
                enemiesSpawned++;
            }
        }
    }
}
