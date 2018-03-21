using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Button[] actionButtons;

    private static KeyCode action1, action2, action3;

    public static Player Player { get; set; }

    // Use this for initialization
    void Start()
    {
        Player = FindObjectOfType<Player>();
        InitializeInputButtons();
    }

    private void InitializeInputButtons()
    {
        // Key binds
        action1 = KeyCode.Alpha1;
        action2 = KeyCode.Alpha2;
        action3 = KeyCode.Alpha3;


    }
    void Update()
    {
        if (Input.GetKeyDown(action1))
        {
            ActionButtonClicked(0);
        }
        if (Input.GetKeyDown(action2))
        {
            ActionButtonClicked(1);
        }
        if (Input.GetKeyDown(action3))
        {
            ActionButtonClicked(2);
        }
    }

    private void ActionButtonClicked(int btnIndex)
    {
        actionButtons[btnIndex].onClick.Invoke();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        TargetiingHandler();
    }

    private void TargetiingHandler()
    {
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E)) && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);

            if (hits.Any())
            {
                foreach (var c in hits)
                {
                    if (c.transform.tag == "Enemy")
                    {
                        if (Player.Target != null)
                        {
                            Player.Target.SendMessage("IsNotTargetting");
                        }
                        Player.Target = c.transform;
                        c.transform.SendMessage("IsTargetting");
                    }
                    else if (c.transform.tag == "Hitbox" && c.transform.parent.tag == "Enemy")
                    {
                        if (Player.Target != null)
                        {
                            Player.Target.SendMessage("IsNotTargetting");
                        }
                        Player.Target = c.transform.parent;
                        c.transform.parent.SendMessage("IsTargetting");
                    }
                    else
                    {
                        if (Player.Target != null)
                        {
                            Player.Target.SendMessage("IsNotTargetting");
                        }
                        Player.Target = null;
                    }
                }
            }
            else
            {
                if (Player.Target != null)
                {
                    Player.Target.SendMessage("IsNotTargetting");
                }
                Player.Target = null;
            }
        }
        if (Player.Target != null &&
                 (Vector2.Distance(Player.transform.position, Player.Target.position) > Player.FieldOfView))
        {
            Player.Target.SendMessage("IsNotTargetting");
            Player.Target = null;
        }
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
