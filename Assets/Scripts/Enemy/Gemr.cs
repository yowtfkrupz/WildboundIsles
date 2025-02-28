using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gemr : MonoBehaviour
{
    public float damage; // Poškození projektilu
    public Collider hunterCollider; // Hunterùv collider, který chceme ignorovat

    private void Start()
    {
        if (hunterCollider != null)
        {
            Collider projectileCollider = GetComponent<Collider>();
            if (projectileCollider != null)
            {
                Physics.IgnoreCollision(projectileCollider, hunterCollider);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player playerScript = other.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damage);
                Debug.Log($"Projectile hit the player! Player took {damage} damage.");
            }
        }
        Destroy(gameObject);
    }
}
