using UnityEngine;
using UnityEngine.UI;

public class Enemy : Character
{
    /// <summary>
    ///     The enemys current state
    /// </summary>
    private IState currentState;

    [SerializeField]
    private float initAggroRange;

    /// <summary>
    ///     A canvas for the target circle
    /// </summary>
    public Canvas ownCanvas;

    /// <summary>
    ///     The enemys attack range
    /// </summary>
    public float AttackRange { get; set; }

    /// <summary>
    ///     How much time has passed since the last attack
    /// </summary>
    public float AttackTime { get; set; }

    public Vector3 StartPosition { get; set; }

    public float AggroRange { get; set; }

    public bool InRange
    {
        get { return Vector2.Distance(transform.position, Target.position) < AggroRange; }
    }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        ownCanvas = GetComponentInChildren<Canvas>();

        StartPosition = transform.position;
        AggroRange = initAggroRange;
        AttackRange = 1;
        ChangeState(new IdleState());
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        FlipHorizontal(1f);

        if (!IsAttacking)
            AttackTime += Time.deltaTime;

        currentState.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (Health.CurrentValue == 0)
            DestroyObject(gameObject);
    }


    /// <summary>
    ///     Changes the enemys state
    /// </summary>
    /// <param name="newState">The new state</param>
    public void ChangeState(IState newState)
    {
        if (currentState != null) //Makes sure we have a state before we call exit
            currentState.Exit();

        //Sets the new state
        currentState = newState;

        //Calls enter on the new state
        currentState.Enter(this);
    }

    /// <summary>
    ///     Makes the enemy take damage when hit and sets target
    /// </summary>
    /// <param name="damage"></param>
    public override void TakeDamage(float damage, Transform source)
    {
        SetTarget(source);

        base.TakeDamage(damage, source);
    }

    public void SetTarget(Transform target)
    {
        if (Target == null && !(currentState is EvadeState))
        {
            var distance = Vector2.Distance(transform.position, target.position);
            AggroRange = initAggroRange;
            AggroRange += distance;
            Target = target;
        }
    }

    public void Reset()
    {
        Target = null;
        AggroRange = initAggroRange;
        Health.CurrentValue = Health.MaxValue;
    }

    public void IsTargetting()
    {
        foreach (Transform child in ownCanvas.transform)
            if (child.tag == "TargettingImage")
                child.GetComponent<Image>().enabled = true;
    }


    public void IsNotTargetting()
    {
        foreach (Transform child in ownCanvas.transform)
            if (child.tag == "TargettingImage")
                child.GetComponent<Image>().enabled = false;
    }
}