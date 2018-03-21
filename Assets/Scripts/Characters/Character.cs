using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

public abstract class Character : MonoBehaviour
{

    public Stat Health;
    public float MaxHealth;
    public float Speed;
    public float MeleeDamage;
    [Range(0f, 100f)]
    public float FieldOfView;

    /// <summary>
    /// The character's target
    /// </summary>
    public Transform Target { get; set; }

    protected Vector2 velocity;
    protected Rigidbody2D rigid;
    protected Animator animator;
    protected SpriteRenderer render;
    protected Vector2 direction;
    protected Coroutine spellRoutine;

    /// <summary>
    /// Indicates if character is moving or not
    /// </summary>
    public bool IsMoving
    {
        get
        {
            return Mathf.Abs(Velocity.x) > 0 || Mathf.Abs(Velocity.y) > 0;
        }
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

    public Animator Animator
    {
        get { return animator; }
        set { animator = value; }
    }

    /// <summary>
    /// indicates if the character is attacking or not
    /// </summary>
    public bool IsAttacking = false;

    /// <summary>
    /// indicates if the character is casting spell
    /// </summary>
    public bool IsCasting = false;

    // Use this for initialization
    protected virtual void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        render = GetComponent<SpriteRenderer>();

        Health.Initialize(MaxHealth, MaxHealth);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        HandleLayers();
    }

    protected virtual void FixedUpdate()
    {
        if (!IsAttacking)
        {
            Move();
        }
    }

    public virtual void TakeDamage(float damage, Transform source)
    {
        Animator.SetTrigger("Hit");
        Health.CurrentValue -= damage;
    }    

    protected void Move()
    {      
        Velocity = Direction.normalized * Speed;
        transform.Translate(Velocity * Time.deltaTime);        
    }    

    /// <summary>
    /// Makes sure that the right animation layer is playing
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
    /// Activates an animation layer based on a string
    /// </summary>
    public void ActivateLayer(string layerName)
    {
        for (int i = 0; i < Animator.layerCount; i++)
        {
            Animator.SetLayerWeight(i, 0);
        }

        Animator.SetLayerWeight(Animator.GetLayerIndex(layerName), 1);
    }

    protected bool InLineOfSight()
    {
        if (Target != null)
        {
            //Calculates the target's direction
            var targetDirection = (Target.transform.position - transform.position).normalized;

            //Thorws a raycast in the direction of the target
            var hit = Physics2D.Raycast(transform.position, targetDirection, Vector2.Distance(transform.position, Target.transform.position), 256);

            //If we hit the block, then we cant cast a spell
            if (hit.collider.tag == "wall")
            {
                Debug.Log("Wall in line: " + hit.collider);
                return false;
            }
        }

        //If we didnt hit the block we can cast a spell
        return true;
    }

    protected virtual void FlipHorizontal(float scale)
    {
        if (Velocity.x < 0f)
        {
            transform.localScale = new Vector3(-scale, transform.localScale.y, transform.localScale.z);
        }
        if (Velocity.x > 0f)
        {
            transform.localScale = new Vector3(scale, transform.localScale.y, transform.localScale.z);
        }
    }
}
