using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class CavePainter : MonoBehaviour
{

    public GameObject floorPrefab;
    public GameObject floor2Prefab;
    public GameObject floorGreenPrefab;
    public GameObject wallPrefab;
    public GameObject wallGreenPrefab;
    public GameObject wallDownPrefab;
    public GameObject wallCornerRightPrefab;
    public GameObject wallCornerLeftPrefab;
    public GameObject wallCornerRightGreenPrefab;
    public GameObject wallCornerLeftGreenPrefab;
    public GameObject wallDownCornerRightPrefab;
    public GameObject wallDownCornerLeftPrefab;
    public GameObject wallDownCornerRightGreenPrefab;
    public GameObject wallDownCornerLeftGreenPrefab;
    public GameObject wallDDownCornerRightPrefab;
    public GameObject wallDDownCornerLeftPrefab;
    public GameObject wallTopCornerRightPrefab;
    public GameObject wallTopCornerLeftPrefab;
    public GameObject wallRightPrefab;
    public GameObject wallLeftPrefab;
    public GameObject wallFirePrefab;
    public GameObject wallVoidPrefab;
    public GameObject wallVoid2Prefab;

    private int width;
    private int height;

    private GenCave generator;
    private byte[,] board;

    // Use this for initialization
    void Start()
    {
        generator = FindObjectOfType<GenCave>();
        generator.boardIsReady += PaintCave;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private bool NotOnBorder(int x, int y)
    {
        return y != 0 && x != 0 && y != height - 1 && x != width - 1;
    }

    private void PaintCave()
    {
        DateTime start = DateTime.UtcNow;

        board = generator.GetBoard();
        width = generator.width;
        height = generator.height;
        if (board != null)
        {
            int wallFireSpawnNeed = 0;
            string caseName = "";
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    if (board[x, y] == 1)
                    {
                        wallFireSpawnNeed++;
                        if (NotOnBorder(x, y) && board[x, y - 1] == 0 && board[x, y + 1] == 1
                            && board[x - 1, y] == 1 && board[x + 1, y] == 1)
                        {
                            if (wallFireSpawnNeed % 5 == 0)
                            {
                                caseName = "wallFire";
                            }
                            else
                            {
                                caseName = "wall";
                            }
                        }
                        else if (NotOnBorder(x, y) && board[x, y - 1] == 1 && board[x, y + 1] == 1
                                    && board[x - 1, y] == 1 && board[x + 1, y] == 0)
                        {
                            caseName = "wallRight";
                        }
                        else if (NotOnBorder(x, y) && board[x, y - 1] == 1 && board[x, y + 1] == 1
                                    && board[x - 1, y] == 0 && board[x + 1, y] == 1)
                        {
                            caseName = "wallLeft";
                        }
                        else if (NotOnBorder(x, y) && board[x, y - 1] == 1
                                     && board[x + 1, y] == 1 && board[x, y + 1] == 1
                                     && board[x - 1, y] == 1 && board[x - 1, y - 1] == 0)
                        {
                            caseName = "wallTopCornerRight";
                        }
                        else if (NotOnBorder(x, y) && board[x, y - 1] == 1
                                    && board[x - 1, y] == 1 && board[x + 1, y + 1] == 1
                                    && board[x + 1, y] == 1 && board[x + 1, y - 1] == 0)
                        {
                            caseName = "wallTopCornerLeft";
                        }
                        else if (NotOnBorder(x, y) && board[x, y + 1] == 1
                                     && board[x - 1, y] == 1 && board[x - 1, y + 1] == 0)
                        {
                            caseName = "wallDDownCornerLeft";
                        }
                        else if (NotOnBorder(x, y) && board[x, y + 1] == 1
                                    && board[x + 1, y] == 1 && board[x + 1, y + 1] == 0)
                        {
                            caseName = "wallDDownCornerRight";
                        }
                        else if (NotOnBorder(x, y) && board[x, y - 1] == 1 && board[x, y + 1] == 1
                                  && board[x - 1, y] == 1 && board[x + 1, y] == 1)
                        {
                            caseName = "wallVoid";
                        }
                        else if (NotOnBorder(x, y) && board[x, y - 1] == 0 && board[x, y + 1] == 1
                                  && board[x - 1, y] == 0 && board[x + 1, y] == 1)
                        {
                            caseName = "wallCornerLeft";
                        }
                        else if (NotOnBorder(x, y) && board[x, y - 1] == 0 && board[x, y + 1] == 1
                                  && board[x - 1, y] == 1 && board[x + 1, y] == 0)
                        {
                            caseName = "wallCornerRight";
                        }
                        else if (NotOnBorder(x, y) && board[x, y - 1] == 1 && board[x, y + 1] == 0
                                    && board[x - 1, y] == 1 && board[x + 1, y] == 1)
                        {
                            caseName = "wallDown";
                        }
                        else if (NotOnBorder(x, y) && board[x, y + 1] == 0 && board[x, y - 1] == 1
                                  && board[x - 1, y] == 0 && board[x + 1, y] == 1)
                        {
                            caseName = "wallDownCornerRight";
                        }
                        else if (NotOnBorder(x, y) && board[x, y + 1] == 0 && board[x, y - 1] == 1
                                   && board[x - 1, y] == 1 && board[x + 1, y] == 0)
                        {
                            caseName = "wallDownCornerLeft";

                        }
                        else if (x == 0 && y == 0)
                        {
                            caseName = "wallCornerLeft";
                        }
                        else if (x == 0 && y == height - 1)
                        {
                            caseName = "wallDownCornerRight";
                        }
                        else if (x == width - 1 && y == height - 1)
                        {
                            caseName = "wallDownCornerLeft";
                        }
                        else if (x == width - 1 && y == 0)
                        {
                            caseName = "wallCornerRight";
                        }
                        else if (y == height - 1)
                        {
                            caseName = "wallDown";
                        }
                        else if (y == 0)
                        {
                            caseName = "wall";
                        }
                        else if (x == 0)
                        {
                            caseName = "wallLeft";
                        }
                        else if (x == width - 1)
                        {
                            caseName = "wallRight";
                        }
                        else
                        {
                            InconsistentTileDetected(x,y);
                            return;                         
                        }
                    }
                    else
                    {
                        caseName = "floor";
                    }
                    DrawElementOfCase(caseName, x, y);
                }
            }
        }

        var difference = start.Subtract(DateTime.UtcNow).TotalSeconds;
        print("MapPainted: " + Math.Abs(difference));
    }

    private void InconsistentTileDetected(int x, int y)
    {
        Debug.Log("InconsistentTileDetected: " + x + ":"+ y);
        board[x, y] = 0;
        var childrensList = new List<GameObject>();
        foreach (Transform child in transform)
            childrensList.Add(child.gameObject);
        childrensList.ForEach(DestroyObject);        
        PaintCave();
    }
    void DrawElementOfCase(string caseName, int x, int y)
    {
        switch (caseName)
        {
            case "wallFire":
                GameObject wallFire = Instantiate(wallFirePrefab, new Vector2(x, y), Quaternion.identity, transform);
                Animator ani = wallFire.GetComponent<Animator>();
                ani.Play(ani.GetCurrentAnimatorStateInfo(0).nameHash, 0, 0.35f * (x % 4));
                break;
            case "wall":
                if (UnityEngine.Random.Range(0, 10) < 8 || y == 0)
                {
                    Instantiate(wallPrefab, new Vector2(x, y), Quaternion.identity, transform);
                }
                else
                {
                    Instantiate(wallGreenPrefab, new Vector2(x, y), Quaternion.identity, transform);
                }
                break;
            case "wallRight":
                GameObject wallRight = Instantiate(wallRightPrefab, new Vector2(x, y), Quaternion.identity, transform);
                wallRight.transform.Translate(Vector2.up * 0.2f);
                break;
            case "wallLeft":
                GameObject wallLeft = Instantiate(wallLeftPrefab, new Vector2(x, y), Quaternion.identity, transform);
                wallLeft.transform.Translate(Vector2.up * 0.2f);
                break;
            case "wallTopCornerRight":
                GameObject wallTopCornerRight = Instantiate(wallTopCornerRightPrefab, new Vector2(x, y), Quaternion.identity, transform);
                wallTopCornerRight.transform.Translate(Vector2.left * 0.06f);
                wallTopCornerRight.transform.Translate(Vector2.down * 0.155f);
                break;
            case "wallTopCornerLeft":
                GameObject wallTopCornerLeft = Instantiate(wallTopCornerLeftPrefab, new Vector2(x, y), Quaternion.identity, transform);
                wallTopCornerLeft.transform.Translate(Vector2.right * 0.06f);
                wallTopCornerLeft.transform.Translate(Vector2.down * 0.155f);
                break;
            case "wallDDownCornerLeft":
                Instantiate(wallDDownCornerLeftPrefab, new Vector2(x, y), Quaternion.identity, transform);
                break;
            case "wallDDownCornerRight":
                Instantiate(wallDDownCornerRightPrefab, new Vector2(x, y), Quaternion.identity, transform);
                break;
            case "wallVoid":
                GameObject wallVoid;
                if (UnityEngine.Random.Range(0, 40) <= 20)
                {
                    wallVoid = Instantiate(wallVoidPrefab, new Vector2(x, y), Quaternion.identity, transform);
                }
                else
                {
                    wallVoid = Instantiate(wallVoid2Prefab, new Vector2(x, y), Quaternion.identity, transform);
                }
                wallVoid.transform.Translate(Vector2.up * 0.2f);
                break;
            case "wallCornerLeft":               
                if (UnityEngine.Random.Range(0, 10) < 8)
                {
                    Instantiate(wallCornerLeftPrefab, new Vector2(x, y), Quaternion.identity, transform);

                }
                else
                {
                    Instantiate(wallCornerLeftGreenPrefab, new Vector2(x, y), Quaternion.identity, transform);
                }

                break;
            case "wallCornerRight":                
                if (UnityEngine.Random.Range(0, 10) < 8)
                {
                    Instantiate(wallCornerRightPrefab, new Vector2(x, y), Quaternion.identity, transform);

                }
                else
                {
                    Instantiate(wallCornerRightGreenPrefab, new Vector2(x, y), Quaternion.identity, transform);
                }
                break;
            case "wallDown":
                Instantiate(wallDownPrefab, new Vector2(x, y), Quaternion.identity, transform);
                break;
            case "wallDownCornerRight":               
                if (UnityEngine.Random.Range(0, 10) < 8)
                {
                    Instantiate(wallDownCornerRightPrefab, new Vector2(x, y), Quaternion.identity, transform);
                }
                else
                {
                    Instantiate(wallDownCornerRightGreenPrefab, new Vector2(x, y), Quaternion.identity, transform);
                }
                break;
            case "wallDownCornerLeft":
                if (UnityEngine.Random.Range(0, 10) < 8)
                {
                    Instantiate(wallDownCornerLeftPrefab, new Vector2(x, y), Quaternion.identity, transform);

                }
                else
                {
                    Instantiate(wallDownCornerLeftGreenPrefab, new Vector2(x, y), Quaternion.identity, transform);
                }
                break;
            case "floor":
                if (UnityEngine.Random.Range(0, 10) < 4)
                {
                    Instantiate(floorPrefab, new Vector2(x, y), Quaternion.Euler(Vector3.forward * 90 * (UnityEngine.Random.Range(0, 10) % 4)), transform);
                }
                else if (UnityEngine.Random.Range(0, 10) < 7)
                {
                    Instantiate(floor2Prefab, new Vector2(x, y), Quaternion.Euler(Vector3.forward * 90 * (UnityEngine.Random.Range(0, 10) % 4)), transform);
                }
                else
                {
                    Instantiate(floorGreenPrefab, new Vector2(x, y), Quaternion.Euler(Vector3.forward * 90 * (UnityEngine.Random.Range(0, 10) % 4)), transform);
                }
                break;
        }
    }
}
