using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class GameManager : MonoBehaviour
{

    public static Player Player { get; set; }    
    private Transform lastPlayersTarget;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        OnTargetClick();
    }

    private void OnTargetClick()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);

            if (hits.Any())
            {
                foreach (var c in hits)
                {
                    if (c.transform.tag == "Enemy")
                    {
                        if (lastPlayersTarget != null)
                        {
                            lastPlayersTarget.SendMessage("IsNotTargetting");
                            lastPlayersTarget = null;
                        }
                        Player.Target = c.transform;
                        lastPlayersTarget = c.transform;
                        c.transform.SendMessage("IsTargetting");
                    }
                    else
                    {
                        Player.Target = null;

                        if (lastPlayersTarget != null)
                        {
                            lastPlayersTarget.SendMessage("IsNotTargetting");
                            lastPlayersTarget = null;
                        }
                    }
                }
            }
            else
            {
                Player.Target = null;

                if (lastPlayersTarget != null)
                {
                    lastPlayersTarget.SendMessage("IsNotTargetting");
                    lastPlayersTarget = null;
                }
            }
        }
    }
}
