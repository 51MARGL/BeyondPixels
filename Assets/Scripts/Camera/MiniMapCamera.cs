using UnityEngine;

public class MiniMapCamera : CameraVirtual
{
    public void SetLimits()
    {
        base.SetLimits(2);
    }
}