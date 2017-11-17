using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class MainCamera : MonoBehaviour
{

    private Player Player;
    private float xMin, xMax, yMin, yMax;
    private GenCave generator;

    public float cameraCoef;
    // Use this for initialization
    void Start()
    {
        generator = FindObjectOfType<GenCave>();
        generator.boardIsReady += SetLimits;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void LateUpdate()
    {
        if (Player != null)
        {
            transform.position = new Vector3(Mathf.Clamp(Player.transform.position.x, xMin, xMax),
                Mathf.Clamp(Player.transform.position.y, yMin, yMax) - 1.5f,
                transform.position.z);
        }
        else
        {
            Player = FindObjectOfType<Player>();
        }
    }

    private void SetLimits()
    {
        var board = generator.GetBoard();
        float camHeight = Camera.main.orthographicSize;
        float camWidth = camHeight * Camera.main.aspect;

        Vector2 minTile = new Vector2(0, 0);
        Vector2 maxTile = new Vector2(generator.width - 1, generator.height - 1);

        xMin = minTile.x + camWidth / cameraCoef;
        xMax = maxTile.x - camWidth / cameraCoef;

        yMin = minTile.y + camHeight / cameraCoef;
        yMax = maxTile.y - camHeight / cameraCoef;

    }
}
