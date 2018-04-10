using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Hitbox" && other.transform.parent.tag != transform.parent.tag)
            other.transform.GetComponentInParent<Character>()
                .TakeDamage(transform.GetComponentInParent<Character>().MeleeDamage, other.transform);
    }
}