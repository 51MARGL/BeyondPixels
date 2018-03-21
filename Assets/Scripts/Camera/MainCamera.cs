using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class MainCamera : CameraVirtual
{

    // Use this for initialization
    protected override void Start()
    {
       base.Start();
       generator.BoardIsReady += SetLimits;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
    }

    private void SetLimits()
    {
        float camHeight = Camera.main.orthographicSize;
        float camWidth = camHeight * Camera.main.aspect;

        Vector2 minTile = new Vector2(0, 0);
        Vector2 maxTile = new Vector2(generator.Width - 1, generator.Height - 1);

        xMin = minTile.x + camWidth / CameraCoef;
        xMax = maxTile.x - camWidth / CameraCoef;

        yMin = minTile.y + camHeight / CameraCoef;
        yMax = maxTile.y - camHeight / CameraCoef;
    }
}
