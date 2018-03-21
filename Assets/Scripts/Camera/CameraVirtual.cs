using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraVirtual : MonoBehaviour {

    protected Player Player;
    protected float xMin, xMax, yMin, yMax;
    protected DungeonProvider generator;

    public float CameraCoef;

    // Use this for initialization
    protected virtual void Start()
    {
        generator = FindObjectOfType<DungeonProvider>();        
    }

    // Update is called once per frame
    protected virtual void Update()
    {

    }

    protected virtual void LateUpdate()
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
}
