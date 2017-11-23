using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCamera : CameaVirtual {

    protected override void Start()
    {
        base.Start();
        generator.boardIsReady += SetLimits;
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
        var board = generator.GetBoard();
        float camHeight = Camera.main.orthographicSize;
        float camWidth = camHeight * Camera.main.aspect;

        Vector2 minTile = new Vector2(0, 0);
        Vector2 maxTile = new Vector2(generator.width - 1, generator.height - 1);

        xMin = minTile.x + camWidth / CameraCoef;
        xMax = maxTile.x - camWidth / CameraCoef;

        yMin = minTile.y + camHeight / CameraCoef * 2;
        yMax = maxTile.y - camHeight / CameraCoef * 2;

    }
}
