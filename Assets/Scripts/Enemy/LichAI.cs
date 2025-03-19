using UnityEngine;
using System.Collections;

public class LichAI : EnemyAI
{
    [Header("Lich Settings")]
    public GameObject magicBallPrefab;
    public Transform magicBallSpawnPoint;
    public float magicBallSpeed = 15f;
    public AudioClip attackSound;
    public AudioSource audioSource;
    public float soulDrainCooldown = 20f;
    private bool canUseSoulDrain = true;

    protected override void Attack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            animator.SetTrigger("Attack");

            if (magicBallPrefab != null && magicBallSpawnPoint != null)
            {
                GameObject magicBall = Instantiate(magicBallPrefab, magicBallSpawnPoint.position, Quaternion.identity);
                Vector3 direction = (player.position - magicBallSpawnPoint.position).normalized;

                Rigidbody rb = magicBall.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = direction * magicBallSpeed;
                }

                MagicBall magicBallScript = magicBall.GetComponent<MagicBall>();
                if (magicBallScript != null)
                {
                    magicBallScript.damage = this.damage;
                }

                if (audioSource != null && attackSound != null)
                {
                    audioSource.PlayOneShot(attackSound);
                }
            }
        }
        if (canUseSoulDrain && Random.value < 0.1f)
        {
            StartCoroutine(UseSoulDrain());
        }
    }

    private IEnumerator UseSoulDrain()
    {
        canUseSoulDrain = false;
        agent.isStopped = true;
        animator.SetTrigger("SpecialAttack");

        Debug.Log("Lich zaèíná vysávat duše!");
        float drainDuration = 4f;
        float damagePerSecond = 5f;
        float healAmount = 0f;

        for (float t = 0; t < drainDuration; t += 1f)
        {
            if (Vector3.Distance(transform.position, player.position) <= attackRange)
            {
                Player playerScript = player.GetComponent<Player>();
                if (playerScript != null)
                {
                    playerScript.TakeDamage(damagePerSecond);
                    healAmount += damagePerSecond;
                }
            }
            yield return new WaitForSeconds(1f);
        }

        health = Mathf.Min(health + healAmount, maxHealth);
        UpdateHealthBar();
        Debug.Log($"Lich se vyléèil o {healAmount} HP!");

        agent.isStopped = false;
        yield return new WaitForSeconds(soulDrainCooldown);
        canUseSoulDrain = true;
    }

    new void Update()
    {
        base.Update();

        float speed = agent.velocity.magnitude;
        animator.SetBool("Walk", currentState == EnemyState.Wander && speed > 0.1f);
        animator.SetBool("Run", currentState == EnemyState.Chase && speed > 0.1f);
    }

    public override void HurtEnemy(float damage)
    {
        base.HurtEnemy(damage);
        animator.SetTrigger("Hit");
    }
}
