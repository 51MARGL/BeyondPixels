using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public List<GameObject> enemyPrefabs;
    [Range(0, 500)]
    public GameObject Player;
    private int enemiesCount;
    private int enemiesSpawned;
    private bool playerSpawned;

    GenCave generator;
    int[,] board;
    int width;
    int height;

    // Use this for initialization
    void Start () {
        enemiesSpawned = 0;
        playerSpawned = false;
        generator = FindObjectOfType<GenCave>();
        generator.boardIsReady += SpawnObjects;
    }

    // Update is called once per frame
    void Update () {

    }

    void SpawnObjects () {
        board = generator.GetBoard();
        width = generator.width;
        height = generator.height;
        enemiesCount = height * width / 150;
        Debug.Log("Enemies-Count:" + enemiesCount);
        SpawnPlayer();
        SpawnEnemies();
    }

    void SpawnPlayer () {
        for (int i = 0; i < width - 1; i++) {
            for (int j = height - 2; j >= 0; j--) {
                if (board[i, j] == 0) {
                    Instantiate(Player, new Vector2(i, j), Quaternion.identity, transform);
                    playerSpawned = true;
                    return;
                }
            }
        }
    }

    void SpawnEnemies () {
        if (board != null) {
            while (enemiesSpawned < enemiesCount) {
                int randomX = Random.Range(0, width - 2);
                int randomY = Random.Range(0, height - 2);
                if (board[randomX, randomY] == 0
                    && Vector2.Distance(Player.transform.position, new Vector2(randomX, randomY)) > 20) {
                    var randomPrefab = enemyPrefabs.ElementAt(0);
                    Instantiate(randomPrefab, new Vector2(randomX, randomY), Quaternion.identity, transform);
                    enemiesSpawned++;
                }
            }
        }
    }
}
