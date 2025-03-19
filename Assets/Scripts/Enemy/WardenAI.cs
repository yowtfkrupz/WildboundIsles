using System.Collections;
using UnityEngine;

public class WardenAI : EnemyAI
{
    [Header("Warden Specific Settings")]
    public float rootChance = 0.3f;
    public float rootDuration = 3f;

    protected override void Attack()
    {
        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                Debug.Log("Warden strikes the player!");
                player.GetComponent<Player>().TakeDamage(damage);

                if (Random.value < rootChance)
                {
                    PerformRootOfEternity();
                }
            }
        }
    }

    private void PerformRootOfEternity()
    {
        Debug.Log("Warden uses Root of Eternity!");

        Player playerScript = player.GetComponent<Player>();
        if (playerScript != null)
        {
            playerScript.ApplyRoot(rootDuration);
        }
    }
}
