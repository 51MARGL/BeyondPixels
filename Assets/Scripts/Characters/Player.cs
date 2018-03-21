using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class Player : Character
{
    private SpellBook spellBook;

    private string[] comboParams;
    private int comboIndex = 0;
    protected float resetAttackTimer;
    public float FireRate = 1;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        spellBook = FindObjectOfType<SpellBook>();

        if (comboParams == null || (comboParams != null && comboParams.Length == 0))
            comboParams = new string[] { "Attack1", "Attack2" };
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        InputHandler();
        AttackComboHandle();
        FlipHorizontal(0.1f);

        // Stop spell casting on move
        if (IsMoving)
        {
            StopSpellCast();
        }
    }

    void InputHandler()
    {
        Direction = Vector2.zero;
        if (Input.GetKey(KeyCode.A))
        {
            Direction += Vector2.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            Direction += Vector2.right;
        }
        if (Input.GetKey(KeyCode.W))
        {
            Direction += Vector2.up;
        }
        if (Input.GetKey(KeyCode.S))
        {
            Direction += Vector2.down;
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
            AttackHandle();
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
        IsCasting = true;
        Animator.SetBool("spellCasting", IsCasting);
        Spell spell = spellBook.CastSpell(spellIndex);
        var spellTarger = Target;
        yield return new WaitForSeconds(spell.CastTime);
        if (spellTarger != null && InLineOfSight())
        {
            var castedSpell = Instantiate(spell.SpellPrefab, spellTarger.position, Quaternion.identity);
            var spellScript = castedSpell.GetComponent<SpellScript>();
            spellScript.Initialize(spellTarger.transform, spell);
            var targetRenderer = spellScript.Target.GetComponent<Renderer>();
            castedSpell.GetComponent<Renderer>().bounds.SetMinMax(targetRenderer.bounds.min * targetRenderer.bounds.size.x, targetRenderer.bounds.max * targetRenderer.bounds.size.y);
        }
        StopSpellCast();
    }

    protected void StopSpellCast()
    {
        spellBook.StopCasting();
        if (spellRoutine != null)
        {
            StopCoroutine(spellRoutine);
            spellRoutine = null;
            IsCasting = false;
            Animator.SetBool("spellCasting", IsCasting);
        }
    }

    private void AttackHandle()
    {
        velocity = Vector2.zero;
        IsAttacking = true;
        Animator.SetTrigger(comboParams[comboIndex]);

        // If combo can loop
        comboIndex = (comboIndex + 1) % comboParams.Length;

        resetAttackTimer = 0f;
    }

    protected void AttackComboHandle()
    {
        // Reset attack

        resetAttackTimer += Time.deltaTime;
        if (resetAttackTimer > FireRate)
        {
            IsAttacking = false;
            comboIndex = 0;
        }
    }
}
