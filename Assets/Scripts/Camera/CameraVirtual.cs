using UnityEngine;

public class CameraVirtual : MonoBehaviour
{
    public float CameraCoef;
    public Transform Target;
    public Vector3 Offset;
    public float SmoothSpeed;
    protected float xMin, xMax, yMin, yMax;

    public MapProvider MapProvider { get; set; }

    // Use this for initialization
    protected virtual void Start()
    {

    }

    // Update is called once per frame
    protected virtual void Update()
    {
    }

    protected virtual void FixedUpdate()
    {
        if (Target != null)
        {
            Vector3 desiredPosition = Target.position + Offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, SmoothSpeed);
            transform.position = new Vector3(Mathf.Clamp(smoothedPosition.x, xMin, xMax),
                                             Mathf.Clamp(smoothedPosition.y, yMin, yMax),
                                             smoothedPosition.z);
        }
    }

    protected virtual void SetLimits(int yCoef)
    {
        var camHeight = Camera.main.orthographicSize;
        var camWidth = camHeight * Camera.main.aspect;

        var minTile = new Vector2(0, 0);
        var maxTile = new Vector2(MapProvider.Width - 1, MapProvider.Height - 1);

        xMin = minTile.x + camWidth / CameraCoef;
        xMax = maxTile.x - camWidth / CameraCoef;

        yMin = minTile.y + camHeight / CameraCoef * yCoef;
        yMax = maxTile.y - camHeight / CameraCoef * yCoef;
    }
}