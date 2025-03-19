using UnityEngine;
using System.Collections;

public class ShamanAI : EnemyAI
{
    [Header("Shaman Settings")]
    public GameObject spikePrefab;
    public float spikeCooldown = 12f;
    private float lastSpikeTime = 0f;

    protected override void Attack()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            PerformMeleeAttack();
        }
        else if (distanceToPlayer > attackRange && Time.time >= lastSpikeTime + spikeCooldown)
        {
            StartCoroutine(UseSpikeAttack());
        }
    }

    private void PerformMeleeAttack()
    {
        lastAttackTime = Time.time;
        agent.isStopped = true;
        animator.SetTrigger("Attack");

        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            Player playerScript = player.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damage);
                Debug.Log("Shaman zasáhl hráèe holí!");
            }
        }

        agent.isStopped = false;
    }

    private IEnumerator UseSpikeAttack()
    {
        lastSpikeTime = Time.time;
        agent.isStopped = true;
        animator.SetTrigger("SpecialAttack");

        yield return new WaitForSeconds(1.5f);

        GameObject spike = Instantiate(spikePrefab, player.position, Quaternion.identity);
        RibCage ribCageScript = spike.GetComponent<RibCage>();

        if (ribCageScript != null)
        {
            ribCageScript.TriggerAppear();
        }

        agent.isStopped = false;
    }

    new void Update()
    {
        base.Update();

        float speed = agent.velocity.magnitude;

        if (currentState == EnemyState.Wander)
        {
            animator.SetBool("Walk", speed > 0.1f);
            animator.SetBool("Run", false);
        }
        else if (currentState == EnemyState.Chase)
        {
            animator.SetBool("Run", speed > 0.1f);
            animator.SetBool("Walk", false);
        }
        else
        {
            animator.SetBool("Walk", false);
            animator.SetBool("Run", false);
        }
    }

    public override void HurtEnemy(float damage)
    {
        base.HurtEnemy(damage);
        animator.SetTrigger("Hit");
    }
}
