using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellScript : MonoBehaviour
{

    private float totalTime;
    private float damage;
    private float duration;
    public Transform Target { get; private set; }
    // Use this for initialization
    void Start()
    {
        StartCoroutine(DestroyOnTimeEnd());
    }

    public void Initialize(Transform target, Spell spell)
    {
        this.Target = target;
        this.damage = spell.Damage;
        this.duration = spell.Duration;
    }

    // Update is called once per frame
    void Update()
    {
        if (Target != null)
        {
            transform.position = Target.position;
        }
    }

    private IEnumerator DestroyOnTimeEnd()
    {
        yield return new WaitForSeconds(duration);
        DestroyObject(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Enemy")
        {
            other.SendMessage("TakeDamage", damage);
        }
    }
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Enemy")
        {
            totalTime += Time.deltaTime;
            if (totalTime > 1)
            {
                other.SendMessage("TakeDamage", damage);
                totalTime = 0;
            }
        }
    }
}
