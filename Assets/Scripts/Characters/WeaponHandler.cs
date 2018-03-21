using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Hitbox" && other.transform.parent.tag != transform.parent.tag)
        {
            other.transform.GetComponentInParent<Character>().TakeDamage(transform.GetComponentInParent<Character>().MeleeDamage, other.transform);
        }
    }
}
