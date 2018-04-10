using System.Collections;
using UnityEngine;

public class SpellScript : MonoBehaviour
{
    private float damage;
    private float duration;

    private float totalTime;

    public Transform Target { get; private set; }

    // Use this for initialization
    private void Start()
    {
        StartCoroutine(DestroyOnTimeEnd());
    }

    public void Initialize(Transform target, Spell spell)
    {
        Target = target;
        damage = spell.Damage;
        duration = spell.Duration;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Target != null)
            transform.position = Target.position;
    }

    private IEnumerator DestroyOnTimeEnd()
    {
        yield return new WaitForSeconds(duration);
        DestroyObject(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Enemy")
            other.transform.GetComponent<Character>().TakeDamage(damage, FindObjectOfType<Player>().transform);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Enemy")
        {
            totalTime += Time.deltaTime;
            if (totalTime > 1)
            {
                other.transform.GetComponent<Character>().TakeDamage(damage, FindObjectOfType<Player>().transform);
                totalTime = 0;
            }
        }
    }
}