using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class CavePainter : MonoBehaviour
{
    public GameObject floor2Prefab;
    public GameObject floorGreenPrefab;
    public GameObject floorPrefab;
    public GameObject wallCornerLeftGreenPrefab;
    public GameObject wallCornerLeftPrefab;
    public GameObject wallCornerRightGreenPrefab;
    public GameObject wallCornerRightPrefab;
    public GameObject wallDDownCornerLeftPrefab;
    public GameObject wallDDownCornerRightPrefab;
    public GameObject wallDownCornerLeftGreenPrefab;
    public GameObject wallDownCornerLeftPrefab;
    public GameObject wallDownCornerRightGreenPrefab;
    public GameObject wallDownCornerRightPrefab;
    public GameObject wallDownPrefab;
    public GameObject wallFirePrefab;
    public GameObject wallGreenPrefab;
    public GameObject wallLeftPrefab;
    public GameObject wallPrefab;
    public GameObject wallRightPrefab;
    public GameObject wallTopCornerLeftPrefab;
    public GameObject wallTopCornerRightPrefab;
    public GameObject wallVoid2Prefab;
    public GameObject wallVoidPrefab;

    public MapProvider MapProvider { get; set; }

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    /// <summary>
    ///     Checks map boundaries
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool NotOnBorder(int x, int y)
    {
        return y != 0 && x != 0 && y != MapProvider.Height - 1 && x != MapProvider.Width - 1;
    }

    /// <summary>
    ///     Starts to paint cellular map
    /// </summary>
    public void PaintCave()
    {
        var start = DateTime.UtcNow;

        var board = MapProvider.Map;
        var mapWidth = MapProvider.Width;
        var mapHeight = MapProvider.Height;
        if (board != null)
        {         
            for (var x = 0; x < mapWidth; x++)
                for (var y = mapHeight - 1; y >= 0; y--)
                {
                    var caseName = "";
                    if (!board[x, y])
                    {                        
                        if (NotOnBorder(x, y) && board[x, y - 1] && !board[x, y + 1]
                            && !board[x - 1, y] && !board[x + 1, y])
                        {
                            if (Random.Range(0,10) > 8)
                                caseName = "wallFire";
                            else
                                caseName = "wall";
                        }
                        else if (NotOnBorder(x, y) && !board[x, y - 1] && !board[x, y + 1]
                                 && !board[x - 1, y] && board[x + 1, y])
                        {
                            caseName = "wallRight";
                        }
                        else if (NotOnBorder(x, y) && !board[x, y - 1] && !board[x, y + 1]
                                 && board[x - 1, y] && !board[x + 1, y])
                        {
                            caseName = "wallLeft";
                        }
                        else if (NotOnBorder(x, y) && !board[x, y - 1]
                                                   && !board[x + 1, y] && !board[x, y + 1]
                                                   && !board[x - 1, y] && board[x - 1, y - 1])
                        {
                            caseName = "wallTopCornerRight";
                        }
                        else if (NotOnBorder(x, y) && !board[x, y - 1]
                                                   && !board[x - 1, y] && !board[x + 1, y + 1]
                                                   && !board[x + 1, y] && board[x + 1, y - 1])
                        {
                            caseName = "wallTopCornerLeft";
                        }
                        else if (NotOnBorder(x, y) && !board[x, y + 1]
                                                   && !board[x - 1, y] && board[x - 1, y + 1])
                        {
                            caseName = "wallDDownCornerLeft";
                        }
                        else if (NotOnBorder(x, y) && !board[x, y + 1]
                                                   && !board[x + 1, y] && board[x + 1, y + 1])
                        {
                            caseName = "wallDDownCornerRight";
                        }
                        else if (NotOnBorder(x, y) && !board[x, y - 1] && !board[x, y + 1]
                                 && !board[x - 1, y] && !board[x + 1, y])
                        {
                            caseName = "wallVoid";
                        }
                        else if (NotOnBorder(x, y) && board[x, y - 1] && !board[x, y + 1]
                                 && board[x - 1, y] && !board[x + 1, y])
                        {
                            caseName = "wallCornerLeft";
                        }
                        else if (NotOnBorder(x, y) && board[x, y - 1] && !board[x, y + 1]
                                 && !board[x - 1, y] && board[x + 1, y])
                        {
                            caseName = "wallCornerRight";
                        }
                        else if (NotOnBorder(x, y) && !board[x, y - 1] && board[x, y + 1]
                                 && !board[x - 1, y] && !board[x + 1, y])
                        {
                            caseName = "wallDown";
                        }
                        else if (NotOnBorder(x, y) && board[x, y + 1] && !board[x, y - 1]
                                 && board[x - 1, y] && !board[x + 1, y])
                        {
                            caseName = "wallDownCornerRight";
                        }
                        else if (NotOnBorder(x, y) && board[x, y + 1] && !board[x, y - 1]
                                 && !board[x - 1, y] && board[x + 1, y])
                        {
                            caseName = "wallDownCornerLeft";
                        }
                        else if (x == 0 && y == 0)
                        {
                            caseName = "wallCornerLeft";
                        }
                        else if (x == 0 && y == mapHeight - 1)
                        {
                            caseName = "wallDownCornerRight";
                        }
                        else if (x == mapWidth - 1 && y == mapHeight - 1)
                        {
                            caseName = "wallDownCornerLeft";
                        }
                        else if (x == mapWidth - 1 && y == 0)
                        {
                            caseName = "wallCornerRight";
                        }
                        else if (y == mapHeight - 1)
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
                        else if (x == mapWidth - 1)
                        {
                            caseName = "wallRight";
                        }
                        else
                        {
                            InconsistentTileDetected(x, y);
                            caseName = "";
                        }
                    }

                    DrawElementOfCase(caseName, x, y);
                }
        }

        var difference = start.Subtract(DateTime.UtcNow).TotalSeconds;
        print("MapPainted: " + Math.Abs(difference));
    }

    /// <summary>
    ///     Logging on map fail generation
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private void InconsistentTileDetected(int x, int y)
    {
        Debug.Log("InconsistentTileDetected: " + x + ":" + y);
    }

    /// <summary>
    ///     Instantiates proper prefabs on given coordinates 
    /// </summary>
    /// <param name="caseName"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private void DrawElementOfCase(string caseName, int x, int y)
    {
        switch (caseName)
        {
            case "wallFire":
                var wallFire = Instantiate(wallFirePrefab, new Vector2(x, y), Quaternion.identity, transform);
                var ani = wallFire.GetComponent<Animator>();
                ani.Play(ani.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0.35f * (x % 4));
                break;
            case "wall":
                if (Random.Range(0, 10) < 8 || y == 0)
                    Instantiate(wallPrefab, new Vector2(x, y), Quaternion.identity, transform);
                else
                    Instantiate(wallGreenPrefab, new Vector2(x, y), Quaternion.identity, transform);
                break;
            case "wallRight":
                var wallRight = Instantiate(wallRightPrefab, new Vector2(x, y), Quaternion.identity, transform);
                wallRight.transform.Translate(Vector2.up * 0.2f);
                break;
            case "wallLeft":
                var wallLeft = Instantiate(wallLeftPrefab, new Vector2(x, y), Quaternion.identity, transform);
                wallLeft.transform.Translate(Vector2.up * 0.2f);
                break;
            case "wallTopCornerRight":
                var wallTopCornerRight = Instantiate(wallTopCornerRightPrefab, new Vector2(x, y), Quaternion.identity,
                    transform);
                wallTopCornerRight.transform.Translate(Vector2.left * 0.06f);
                wallTopCornerRight.transform.Translate(Vector2.down * 0.155f);
                break;
            case "wallTopCornerLeft":
                var wallTopCornerLeft = Instantiate(wallTopCornerLeftPrefab, new Vector2(x, y), Quaternion.identity,
                    transform);
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
                if (Random.Range(0, 40) <= 20)
                    wallVoid = Instantiate(wallVoidPrefab, new Vector2(x, y), Quaternion.identity, transform);
                else
                    wallVoid = Instantiate(wallVoid2Prefab, new Vector2(x, y), Quaternion.identity, transform);
                wallVoid.transform.Translate(Vector2.up * 0.2f);
                break;
            case "wallCornerLeft":
                if (Random.Range(0, 10) < 8)
                    Instantiate(wallCornerLeftPrefab, new Vector2(x, y), Quaternion.identity, transform);
                else
                    Instantiate(wallCornerLeftGreenPrefab, new Vector2(x, y), Quaternion.identity, transform);
                break;
            case "wallCornerRight":
                if (Random.Range(0, 10) < 8)
                    Instantiate(wallCornerRightPrefab, new Vector2(x, y), Quaternion.identity, transform);
                else
                    Instantiate(wallCornerRightGreenPrefab, new Vector2(x, y), Quaternion.identity, transform);
                break;
            case "wallDown":
                Instantiate(wallDownPrefab, new Vector2(x, y), Quaternion.identity, transform);
                break;
            case "wallDownCornerRight":
                if (Random.Range(0, 10) < 8)
                    Instantiate(wallDownCornerRightPrefab, new Vector2(x, y), Quaternion.identity, transform);
                else
                    Instantiate(wallDownCornerRightGreenPrefab, new Vector2(x, y), Quaternion.identity, transform);
                break;
            case "wallDownCornerLeft":
                if (Random.Range(0, 10) < 8)
                    Instantiate(wallDownCornerLeftPrefab, new Vector2(x, y), Quaternion.identity, transform);
                else
                    Instantiate(wallDownCornerLeftGreenPrefab, new Vector2(x, y), Quaternion.identity, transform);
                break;
            default:
                break;
        }

        //case "floor": on every tile we want to spawn floor prefab (for proper wall transparency)
        if (Random.Range(0, 10) < 4)
            Instantiate(floorPrefab, new Vector2(x, y),
                Quaternion.Euler(Vector3.forward * 90 * (Random.Range(0, 10) % 4)), transform);
        else if (Random.Range(0, 10) < 7)
            Instantiate(floor2Prefab, new Vector2(x, y),
                Quaternion.Euler(Vector3.forward * 90 * (Random.Range(0, 10) % 4)), transform);
        else
            Instantiate(floorGreenPrefab, new Vector2(x, y),
                Quaternion.Euler(Vector3.forward * 90 * (Random.Range(0, 10) % 4)), transform);
    }
}