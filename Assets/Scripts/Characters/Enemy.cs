using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character {

    public float fireRate = 2;
    public GameObject target;
    private float attackCast = 0f;
    public float attackRange = 1f;
    private float tChange = 0; // force new direction in the first Update
    private float randomX;
    private float randomY;

    // Use this for initialization
    protected override void Start () {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update () {
        base.Update();
        FlipHorizontal();     
    }

    void FixedUpdate () {
        if (target != null) {
            var targetDir = target.transform.position - transform.position;
            velocity = targetDir.normalized * speed;
            if (Vector2.Distance(target.transform.position, transform.position) < attackRange) {
                Attack();
            } else {
                transform.position += new Vector3(velocity.x, velocity.y, 0) * Time.deltaTime;
            }
        } else {
            MoveAtRandom();
        }

        if (health.CurrentValue == 0) {
            DestroyObject(gameObject);
        }
    }

    private void Attack () {
        if (Time.time >= attackCast) {
            animator.SetTrigger("Attack");
            attackCast = Time.time + Random.Range(fireRate - 0.5f, fireRate + 0.5f);
        }
    }

    void MoveAtRandom () {
        if (Time.time >= tChange) {
            randomX = Random.Range(-100, 100);
            randomY = Random.Range(-100, 100);
            tChange = Time.time + Random.Range(0.5f, 1.5f);
        }
        velocity = new Vector2(randomX, randomY).normalized * speed;
        transform.Translate(velocity * Time.deltaTime);
    }

    void OnTriggerEnter2D (Collider2D other) {
        if (other.tag == "Player" && animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) {
            other.SendMessage("TakeDamage", 5);
        }
    }

    void OnCollisionEnter2D (Collision2D coll) {
        if (coll.gameObject.tag == "Wall") {
            randomX = -randomX;
            randomY = -randomY;
        }
    }

    protected void FlipHorizontal () {
        if (velocity.x < 0f) {
            transform.localScale = new Vector3(-1f, transform.localScale.y, transform.localScale.z);
        }
        if (velocity.x > 0f) {
            transform.localScale = new Vector3(1f, transform.localScale.y, transform.localScale.z);
        }
    }
}
