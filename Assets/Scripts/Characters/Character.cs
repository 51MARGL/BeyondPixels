using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour {

    public Stat health;
    public float maxHealth = 100;
    public float speed = 10;
    public Transform Target { get; set; }
    protected Vector2 velocity;
    protected Rigidbody2D rigid;
    protected Animator animator;
    protected SpriteRenderer render;

    // Use this for initialization
    protected virtual void Start () {
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        render = GetComponent<SpriteRenderer>();

        health.Initialize(maxHealth, maxHealth);
    }

    // Update is called once per frame
    protected virtual void Update () {
        
    }

    protected void TakeDamage (int damage) {
        animator.SetTrigger("Hit");
        health.CurrentValue -= damage;
    }
}
