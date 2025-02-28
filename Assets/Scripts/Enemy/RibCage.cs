using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RibCage : MonoBehaviour
{
    public float damage;

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
