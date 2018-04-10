using System.Linq;
using UnityEngine;

public abstract class Character : MonoBehaviour
{



    public Stat Health;

    /// <summary>
    ///     indicates if the character is attacking or not
    /// </summary>
    public bool IsAttacking = false;

    /// <summary>
    ///     indicates if the character is casting spell
    /// </summary>
    public bool IsCasting = false;

    public float MaxHealth;
    public float MeleeDamage;
    public SpriteRenderer Render { get; set; }
    public Rigidbody2D Rigid { get; set; }
    public Animator Animator { get; set; }

    public float Speed;
    protected Vector2 velocity;
    protected Vector2 direction;
    protected Coroutine spellRoutine;

    /// <summary>
    ///     The character's target
    /// </summary>
    public Transform Target { get; set; }

    /// <summary>
    ///     Indicates if character is moving or not
    /// </summary>
    public bool IsMoving
    {
        get { return Mathf.Abs(Velocity.x) > 0 || Mathf.Abs(Velocity.y) > 0; }
    }

    public Vector2 Velocity
    {
        get { return velocity; }
        set { velocity = value; }
    }

    public Vector2 Direction
    {
        get { return direction; }
        set { direction = value; }
    }

    // Use this for initialization
    protected virtual void Start()
    {
        Rigid = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        Render = GetComponent<SpriteRenderer>();

        Health.Initialize(MaxHealth, MaxHealth);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        HandleLayers();
    }

    protected virtual void FixedUpdate()
    {
        if (!IsAttacking) Move();
    }

    /// <summary>
    ///     Take damage method. Get called by weapon objects
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="source"></param>
    public virtual void TakeDamage(float damage, Transform source)
    {
        Animator.SetTrigger("Hit");
        Health.CurrentValue -= damage;
    }

    /// <summary>
    ///     Moves object in direction
    /// </summary>
    protected void Move()
    {
        Velocity = Direction.normalized * Speed;
        transform.Translate(Velocity * Time.deltaTime);
    }

    /// <summary>
    ///     Makes sure that the right animation layer is playing
    /// </summary>
    public void HandleLayers()
    {
        //Checks if we are moving or standing still, if we are moving then we need to play the move animation
        if (IsMoving)
        {
            ActivateLayer("RunLayer");

            //Sets the animation parameter so that he faces the correct direction
            Animator.SetFloat("velocity.x", Mathf.Abs(velocity.x));
            Animator.SetFloat("velocity.y", velocity.y);
        }
        else if (IsAttacking)
        {
            ActivateLayer("AttackLayer");
        }
        else if (IsCasting)
        {
            ActivateLayer("CastSpellLayer");
        }
        else
        {
            //Makes sure that we will go back to idle when we aren't pressing any keys.
            ActivateLayer("IdleLayer");
        }
    }

    /// <summary>
    ///     Activates an animation layer based on a string
    /// </summary>
    public void ActivateLayer(string layerName)
    {
        for (var i = 0; i < Animator.layerCount; i++)
            Animator.SetLayerWeight(i, 0);

        Animator.SetLayerWeight(Animator.GetLayerIndex(layerName), 1);
    }

    /// <summary>
    ///     Checks if target is in line of sight
    /// </summary>
    /// <returns></returns>
    public bool InLineOfSight()
    {
        if (Target != null)
        {
            //Calculates the target's direction
            var targetDirection = (Target.transform.position - transform.position).normalized;

            //Thorws a raycast in the direction of the target
            var hits = Physics2D.RaycastAll(transform.position, targetDirection,
                Vector2.Distance(transform.position, Target.transform.position), 1);
            if (hits.Any())
            {
                //If we hit the block, then we cant cast a spell
                foreach (var hit in hits)
                    if (hit.transform != null && hit.transform.tag == "Wall")
                        return false;

            }

        }

        //If we didnt hit the block we can cast a spell
        return true;
    }

    protected virtual void FlipHorizontal(float scale)
    {
        if (Velocity.x < 0f)
            transform.localScale = new Vector3(-scale, transform.localScale.y, transform.localScale.z);
        if (Velocity.x > 0f)
            transform.localScale = new Vector3(scale, transform.localScale.y, transform.localScale.z);
    }
}