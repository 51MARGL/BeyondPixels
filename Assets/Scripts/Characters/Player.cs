using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character {

    private bool attackStart = false;

    // Use this for initialization
    protected override void Start () {
        health = GameObject.FindGameObjectWithTag("PlayerHealth").GetComponent<Stat>();
        base.Start();
    }

    // Update is called once per frame
    protected override void Update () {
        base.Update();
        InputHandler();
        AnimationHandler();
        AttackHandler();
        FlipHorizontal();
    }

    void FixedUpdate () {
        if (!base.animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) {
            velocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized * speed;
            rigid.transform.Translate(velocity * Time.deltaTime);
        }
    }

    void InputHandler () {
        if (Input.GetKeyDown(KeyCode.KeypadPlus)) {
            health.CurrentValue += 10;
        } else if (Input.GetKeyDown(KeyCode.KeypadMinus)) {
            health.CurrentValue -= 10;
        }
        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space)) {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("player-attack-1")) {
                velocity = Vector2.zero;                
                animator.SetBool("inCombo", true);
                animator.SetBool("inRepeat", false);
            } else if (animator.GetCurrentAnimatorStateInfo(0).IsName("player-attack-2")) {
                velocity = Vector2.zero;
                animator.SetBool("inCombo", false);
                animator.SetBool("inRepeat", true);
            } else {
                velocity = Vector2.zero;
                attackStart = true;
            }
        }
    }

    void AttackHandler () {
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) {
            return;
        } else {
            animator.SetBool("inCombo", false);
            animator.SetBool("inRepeat", false);
        }
        if (attackStart) {
            animator.SetTrigger("combo-start");
            attackStart = false;
        }
    }

    void AnimationHandler () {
        animator.SetFloat("speed", Mathf.Abs(velocity.x) + Mathf.Abs(velocity.y));
        animator.SetFloat("velocity.x", Mathf.Abs(velocity.x));
        animator.SetFloat("velocity.y", velocity.y);
    }

    void OnTriggerEnter2D (Collider2D other) {
        if (other.tag == "Enemy") {
            other.SendMessage("TakeDamage", 10);
        }
    }

    protected void FlipHorizontal () {
        if (velocity.x < 0f) {
            transform.localScale = new Vector3(-0.1f, transform.localScale.y, transform.localScale.z);
        }
        if (velocity.x > 0f) {
            transform.localScale = new Vector3(0.1f, transform.localScale.y, transform.localScale.z);
        }
    }
}
