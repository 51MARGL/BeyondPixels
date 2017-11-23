using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour
{

    public Stat Health;
    public float MaxHealth;
    public float Speed;
    public float MeleeDamage;
    [Range(0f, 100f)]
    public float FieldOfView;
    public Transform Target { get; set; }

    protected Vector2 velocity;
    protected Rigidbody2D rigid;
    protected Animator animator;
    protected SpriteRenderer render;
    protected Vector2 direction;
    protected Coroutine spellRoutine;
    // Use this for initialization
    protected virtual void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        render = GetComponent<SpriteRenderer>();

        Health.Initialize(MaxHealth, MaxHealth);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            Move();
        }
    }

    protected void TakeDamage(int damage)
    {
        animator.SetTrigger("Hit");
        Health.CurrentValue -= damage;
    }

    protected virtual void StopSpellCast()
    {
        if (spellRoutine != null)
        {
            StopCoroutine(spellRoutine);
            spellRoutine = null;
            animator.SetBool("spellCasting", false);
        }
    }

    protected void Move()
    {
        velocity = direction.normalized * Speed;
        transform.Translate(velocity * Time.deltaTime);
        if ((Mathf.Abs(velocity.x) > 0 || Mathf.Abs(velocity.y) > 0))
        {
            StopSpellCast();
        }
    }
}
