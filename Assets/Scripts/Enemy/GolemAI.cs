using UnityEngine;
using System.Collections.Generic;

public class GolemAI : EnemyAI
{
    [Header("Golem Settings")]
    public float meleeDamage = 20f;
    public float meleeRange = 2f;
    public float areaAttackDamage = 30f;
    public float areaAttackRadius = 5f;
    public float areaAttackCooldown = 10f;
    private float lastAreaAttackTime;

    [Header("Golem Animations")]
    public List<string> meleeAttackTriggers = new List<string> { "Attack1", "Attack2" };

    new void Update()
    {
        base.Update();

        float speed = agent.velocity.magnitude;

        if (currentState == EnemyState.Wander && speed > 0.1f)
        {
            animator.SetBool("Walk", true);
            animator.SetBool("Run", false);
        }
        else if (currentState == EnemyState.Chase && speed > 0.1f)
        {
            animator.SetBool("Run", true);
            animator.SetBool("Walk", false);
        }
        else
        {
            animator.SetBool("Walk", false);
            animator.SetBool("Run", false);
        }
    }

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
            string attackTrigger = meleeAttackTriggers[Random.Range(0, meleeAttackTriggers.Count)];
            animator.SetTrigger(attackTrigger);

            Player playerScript = player.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(meleeDamage);
                Debug.Log($"Golem used melee attack ({attackTrigger})!");
            }
        }
    }

    private void PerformAreaAttack()
    {
        animator.SetTrigger("AreaAttack");
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

    public override void HurtEnemy(float damage)
    {
        base.HurtEnemy(damage);
        animator.SetTrigger("Hit");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, areaAttackRadius);
    }
}
