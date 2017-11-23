﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : Character
{

    public float fireRate = 2;
    public float attackRange = 1f;
    private float tChange; // force new direction in the first Update
    private float randomX;
    private float randomY;

    public Canvas ownCanvas;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        ownCanvas = GetComponentInChildren<Canvas>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        FlipHorizontal();

        if (Health.CurrentValue == 0)
        {
            DestroyObject(gameObject);
        }
    }

    void FixedUpdate()
    {
        MovementHandle();
    }


    private void MovementHandle()
    {
        if (Vector2.Distance(this.transform.position, GameManager.Player.transform.position) < FieldOfView)
        {
            Target = GameManager.Player.transform;
        }
        else
        {
            Target = null;
        }

        if (Target != null)
        {
            if (Target != null 
                && Vector2.Distance(Target.position, transform.position) < attackRange
                && spellRoutine == null)
            {
                direction = Vector2.zero;
                spellRoutine = StartCoroutine(Attack());
            }
            else if (Target != null 
                && Vector2.Distance(Target.position, transform.position) > attackRange)
            {
                FollowTarget();
            }
        }
        else
        {
            MoveAtRandom();
        }
    }

    private void FollowTarget()
    {
        direction = Target.position - transform.position;
    }

    private IEnumerator Attack()
    {
        yield return new WaitForSeconds(Random.Range(fireRate, fireRate + 0.5f));
        animator.SetTrigger("Attack");
        StopSpellCast();
    }


    void MoveAtRandom()
    {
        if (Time.time >= tChange)
        {
            randomX = Random.Range(-100, 100);
            randomY = Random.Range(-100, 100);
            tChange = Time.time + Random.Range(0.5f, 1.5f);
        }
        direction = new Vector2(randomX, randomY);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            other.SendMessage("TakeDamage", MeleeDamage);
        }
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Wall")
        {
            randomX = -randomX;
            randomY = -randomY;
        }
    }

    protected void FlipHorizontal()
    {
        if (velocity.x < 0f)
        {
            transform.localScale = new Vector3(-1f, transform.localScale.y, transform.localScale.z);
        }
        if (velocity.x > 0f)
        {
            transform.localScale = new Vector3(1f, transform.localScale.y, transform.localScale.z);
        }
    }

    public void IsTargetting()
    {
        foreach (Transform child in ownCanvas.transform)
        {
            if (child.tag == "TargettingImage")
            {
                child.GetComponent<Image>().enabled = true;
            }
        }
    }


    public void IsNotTargetting()
    {
        foreach (Transform child in ownCanvas.transform)
        {
            if (child.tag == "TargettingImage")
            {
                child.GetComponent<Image>().enabled = false;
            }
        }
    }
}
