using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sight : MonoBehaviour {

    public Enemy enemy;
	
    void OnTriggerEnter2D(Collider2D other) {
        if (!Physics.Linecast(transform.position, other.transform.position)) {
            if (other.tag == "Player") {
                enemy.target = other.gameObject;
            }
        }
    }

    void OnTriggerExit2D (Collider2D other) {
        if (other.tag == "Player") {
            enemy.target = null;
            return;
        } 
    }
}
