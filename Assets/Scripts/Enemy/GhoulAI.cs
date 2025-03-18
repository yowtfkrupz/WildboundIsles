using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GhoulAI : EnemyAI
{
    [Header("Ghoul Settings")]
    public float healAmount = 5f;

    [Header("Ghoul Sounds")]
    public List<AudioClip> idleSounds;
    public AudioClip chaseSound;
    public AudioClip attackSound;
    public AudioClip hitSound;
    public AudioSource audioSource;

    private bool playedChaseSound = false;

    new void Start()
    {
        base.Start();
        StartCoroutine(PlayIdleSounds());
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

            if (!playedChaseSound && chaseSound != null)
            {
                audioSource.PlayOneShot(chaseSound);
                playedChaseSound = true;
            }
        }
        else
        {
            animator.SetBool("Walk", false);
            animator.SetBool("Run", false);
        }

        if (currentState != EnemyState.Chase)
        {
            playedChaseSound = false;
        }
    }

    protected override void Attack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;

            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");

            if (attackSound != null)
            {
                audioSource.PlayOneShot(attackSound);
            }

            if (Vector3.Distance(transform.position, player.position) <= attackRange)
            {
                Player playerScript = player.GetComponent<Player>();
                if (playerScript != null)
                {
                    playerScript.TakeDamage(damage);
                }
            }

            HealGhoul();
        }
    }

    private void HealGhoul()
    {
        health += healAmount;
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthBar();
    }

    public override void HurtEnemy(float damage)
    {
        base.HurtEnemy(damage);

        animator.ResetTrigger("Hit");
        animator.SetTrigger("Hit");

        if (hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }

    private IEnumerator PlayIdleSounds()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(4f, 8f));

            if (idleSounds.Count > 0 && currentState == EnemyState.Wander)
            {
                AudioClip clip = idleSounds[Random.Range(0, idleSounds.Count)];
                if (clip != null)
                {
                    audioSource.PlayOneShot(clip);
                }
            }
        }
    }
}
