using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HunterAI : EnemyAI
{
    [Header("Hunter Sounds")]
    public List<AudioClip> idleSounds;
    public List<AudioClip> chaseSounds;
    public List<AudioClip> attackSounds;
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

            if (!playedChaseSound && chaseSounds.Count > 0)
            {
                AudioClip chaseClip = chaseSounds[Random.Range(0, chaseSounds.Count)];
                audioSource.PlayOneShot(chaseClip);
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

            if (attackSounds.Count > 0)
            {
                AudioClip attackClip = attackSounds[Random.Range(0, attackSounds.Count)];
                audioSource.PlayOneShot(attackClip);
            }

            if (Vector3.Distance(transform.position, player.position) <= attackRange)
            {
                Player playerScript = player.GetComponent<Player>();
                if (playerScript != null)
                {
                    playerScript.TakeDamage(damage);
                    Debug.Log("Hunter udeřil hráče!");
                }
            }
        }
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
