using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    private static KeyCode action1, action2, action3;

    [SerializeField]
    private Button[] actionButtons;

    public Player Player { get; set; }

    public MapProvider mapProvider;

    // Use this for initialization
    private void Start()
    {
        Player = FindObjectOfType<Player>();
        InitializeInputButtons();

        var mapWidth = Random.Range(50, 200);
        var mapHeight = Random.Range(50, 100);
        var mapFillPercente = Random.Range(49, 53);
        var mapPassRadius = 1;
        mapProvider = new DungeonProvider(mapHeight, mapWidth, mapFillPercente, mapPassRadius);
        mapProvider.MapIsReady += OnMapIsReady;

        var start = DateTime.UtcNow;
        mapProvider.GenerateMap();
        print("MapGenerated: " + Math.Abs(start.Subtract(DateTime.UtcNow).TotalSeconds));

    }

    private void OnMapIsReady()
    {
        var painter = FindObjectOfType<TileMapProvider>();
        painter.MapProvider = mapProvider;
        painter.CreateTileMap();


        var spawner = FindObjectOfType<SpawnManager>();
        spawner.MapProvider = mapProvider;
        spawner.SpawnObjects();

        var miniMapCamera = FindObjectOfType<MiniMapCamera>();
        miniMapCamera.MapProvider = mapProvider;
        miniMapCamera.SetLimits();
        miniMapCamera.Target = this.Player.transform;
    }

    private void InitializeInputButtons()
    {
        // Key binds
        action1 = KeyCode.Alpha1;
        action2 = KeyCode.Alpha2;
        action3 = KeyCode.Alpha3;
    }

    private void Update()
    {
        if (Input.GetKeyDown(action1))
            ActionButtonClicked(0);
        if (Input.GetKeyDown(action2))
            ActionButtonClicked(1);
        if (Input.GetKeyDown(action3))
            ActionButtonClicked(2);
    }

    private void ActionButtonClicked(int btnIndex)
    {
        actionButtons[btnIndex].onClick.Invoke();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        TargetingHandler();
    }

    private void TargetingHandler()
    {
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E)) &&
            !EventSystem.current.IsPointerOverGameObject())
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, 1 << 8); //clickable layer index = 8
            
            if (hit.transform != null)
            {
                if (hit.transform.tag == "Enemy")
                {
                    if (Player.Target != null)
                        Player.Target.SendMessage("IsNotTargetting");

                    Player.Target = hit.transform;
                    hit.transform.SendMessage("IsTargetting");
                }
                else if (hit.transform.tag == "Hitbox" && hit.transform.parent.tag == "Enemy")
                {
                    if (Player.Target != null)
                    {
                        Player.Target.SendMessage("IsNotTargetting");
                    }
                    Player.Target = hit.transform.parent;
                    hit.transform.parent.SendMessage("IsTargetting");
                }
                else
                {
                    if (Player.Target != null)
                        Player.Target.SendMessage("IsNotTargetting");
                    Player.Target = null;
                }
            }
            else
            {
                if (Player.Target != null)
                    Player.Target.SendMessage("IsNotTargetting");
                Player.Target = null;
            }
        }

        if (Player.Target != null &&
            !GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main),
                Player.Target.GetComponent<Character>().Render.bounds)) // if object is not vissible by main camera
        {
            Player.Target.SendMessage("IsNotTargetting");
            Player.Target = null;
        }

        if (EventSystem.current.currentSelectedGameObject != null)
            EventSystem.current.SetSelectedGameObject(null);
    }
}