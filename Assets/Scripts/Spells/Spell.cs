using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : MonoBehaviour
{
    public int CastTime;
    public int Damage;
    public float DurationTime;

    private float totalTime;

    public Transform Target;
    // Use this for initialization
    void Start()
    {
        StartCoroutine(DestroyOnTimeEnd());
    }

    // Update is called once per frame
    void Update()
    {
        if (Target != null)
        {
            transform.position = Target.position;
        }
        else
        {
            DestroyObject(gameObject);
        }
    }

    private IEnumerator DestroyOnTimeEnd()
    {
        yield return new WaitForSeconds(DurationTime);
        DestroyObject(gameObject);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Enemy")
        {
            totalTime += Time.deltaTime;
            if (totalTime > 1)
            {
                other.SendMessage("TakeDamage", Damage);
                totalTime = 0;
            }
        }
    }
}
