using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class Player : Character
{

    private bool attackStart = false;

    private SpellBook spellBook;

    // Use this for initialization
    protected override void Start()
    {
        Health = GameObject.FindGameObjectWithTag("PlayerHealth").GetComponent<Stat>();
        base.Start();
        spellBook = FindObjectOfType<SpellBook>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        InputHandler();
        AnimationHandler();
        AttackHandler();
        FlipHorizontal();
    }

    void InputHandler()
    {
        direction = Vector2.zero;
        if (Input.GetKey(KeyCode.A))
        {
            direction += Vector2.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction += Vector2.right;
        }
        if (Input.GetKey(KeyCode.W))
        {
            direction += Vector2.up;
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction += Vector2.down;
        }
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            Health.CurrentValue += 10;
        }
        else if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            Health.CurrentValue -= 10;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StopSpellCast();
            AttackComboHandle();
        }
    }

    public void StartSpellCast(int spellIndex)
    {
        if (Target != null && spellRoutine == null)
        {
            spellRoutine = StartCoroutine(SpellCast(spellIndex));
        }
    }
    private IEnumerator SpellCast(int spellIndex)
    {
        animator.SetBool("spellCasting", true);
        Spell spell = spellBook.CastSpell(spellIndex);
        var spellTarger = Target;
        yield return new WaitForSeconds(spell.CastTime);
        if (spellTarger != null)
        {
            var castedSpell = Instantiate(spell.SpellPrefab, spellTarger.position, Quaternion.identity);
            var spellScript = castedSpell.GetComponent<SpellScript>();
            spellScript.Initialize(spellTarger.transform, spell);
            var targetRenderer = spellScript.Target.GetComponent<Renderer>();
            castedSpell.GetComponent<Renderer>().bounds.SetMinMax(targetRenderer.bounds.min * targetRenderer.bounds.size.x, targetRenderer.bounds.max * targetRenderer.bounds.size.y);
        }
        StopSpellCast();
    }

    protected override void StopSpellCast()
    {
        spellBook.StopCasting();
        base.StopSpellCast();
    }

    private void AttackComboHandle()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("player-attack-1"))
        {
            velocity = Vector2.zero;
            animator.SetBool("inCombo", true);
            animator.SetBool("inRepeat", false);
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("player-attack-2"))
        {
            velocity = Vector2.zero;
            animator.SetBool("inCombo", false);
            animator.SetBool("inRepeat", true);
        }
        else
        {
            velocity = Vector2.zero;
            attackStart = true;
        }
    }

    void AttackHandler()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            return;
        }
        else
        {
            animator.SetBool("inCombo", false);
            animator.SetBool("inRepeat", false);
        }
        if (attackStart)
        {
            animator.SetTrigger("combo-start");
            attackStart = false;
        }
    }

    void AnimationHandler()
    {
        animator.SetFloat("speed", Mathf.Abs(velocity.x) + Mathf.Abs(velocity.y));
        animator.SetFloat("velocity.x", Mathf.Abs(velocity.x));
        animator.SetFloat("velocity.y", velocity.y);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Enemy")
        {
            other.SendMessage("TakeDamage", MeleeDamage);
        }
    }

    protected void FlipHorizontal()
    {
        if (velocity.x < 0f)
        {
            transform.localScale = new Vector3(-0.1f, transform.localScale.y, transform.localScale.z);
        }
        if (velocity.x > 0f)
        {
            transform.localScale = new Vector3(0.1f, transform.localScale.y, transform.localScale.z);
        }
    }
}
