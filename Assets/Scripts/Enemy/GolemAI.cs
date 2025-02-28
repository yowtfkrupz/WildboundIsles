using UnityEngine;

public class GolemAI : EnemyAI
{
    [Header("Golem Settings")]
    public float meleeDamage = 20f;
    public float meleeRange = 2f;
    public float areaAttackDamage = 30f;
    public float areaAttackRadius = 5f;
    public float areaAttackCooldown = 10f;
    private float lastAreaAttackTime;

    protected override void Attack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformMeleeAttack();
            lastAttackTime = Time.time;

            if (Time.time >= lastAreaAttackTime + areaAttackCooldown)
            {
                PerformAreaAttack();
                lastAreaAttackTime = Time.time;
            }
        }
    }

    private void PerformMeleeAttack()
    {
        if (Vector3.Distance(transform.position, player.position) <= meleeRange)
        {
            Player playerScript = player.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(meleeDamage);
                Debug.Log("Golem performed a melee attack!");
            }
        }
    }

    private void PerformAreaAttack()
    {
        Debug.Log("Golem used an area attack!");

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, areaAttackRadius);
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                Player playerScript = hitCollider.GetComponent<Player>();
                if (playerScript != null)
                {
                    playerScript.TakeDamage(areaAttackDamage);
                    Debug.Log("Player took damage from Golem's area attack!");
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, areaAttackRadius);
    }
}
