using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HunterAI : EnemyAI
{
    [Header("Hunter Sounds")]
    public List<AudioClip> idleSounds; // Seznam náhodných idle zvuků
    public List<AudioClip> chaseSounds; // Seznam zvuků při přechodu do Chase
    public List<AudioClip> attackSounds; // Seznam zvuků při útoku
    public AudioClip hitSound; // Zvuk při zásahu
    public AudioSource audioSource; // Audio komponenta pro přehrávání

    private bool playedChaseSound = false; // Kontrola, zda už chase zvuk hrál

    new void Start()
    {
        base.Start();
        StartCoroutine(PlayIdleSounds()); // Spustíme smyčku pro idle zvuky
    }

    new void Update()
    {
        base.Update();

        float speed = agent.velocity.magnitude;

        // Walk animace loopuje, dokud se nezmění stav na něco jiného
        if (currentState == EnemyState.Wander)
        {
            animator.SetBool("Walk", speed > 0.1f);
            animator.SetBool("Run", false);
        }
        // Run animace loopuje, dokud se nezmění stav na něco jiného
        else if (currentState == EnemyState.Chase)
        {
            animator.SetBool("Run", speed > 0.1f);
            animator.SetBool("Walk", false);

            if (!playedChaseSound && chaseSounds.Count > 0)
            {
                AudioClip chaseClip = chaseSounds[Random.Range(0, chaseSounds.Count)];
                audioSource.PlayOneShot(chaseClip);
                playedChaseSound = true; // Chase zvuk přehrajeme jen jednou
            }
        }
        else // Pokud je jiný stav (Attack, Hit), vypnout Walk i Run
        {
            animator.SetBool("Walk", false);
            animator.SetBool("Run", false);
        }

        // Resetování chase zvuku, pokud Hunter přestane pronásledovat hráče
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

            // Resetujeme trigger, aby se animace mohla znovu spustit
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");

            if (attackSounds.Count > 0)
            {
                AudioClip attackClip = attackSounds[Random.Range(0, attackSounds.Count)];
                audioSource.PlayOneShot(attackClip);
            }

            // ✅ Udělení damage hráči
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

        // Resetujeme trigger, aby se Hit animace mohla spustit vícekrát
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
            yield return new WaitForSeconds(Random.Range(4f, 8f)); // Náhodná prodleva mezi 4–8 sekundami

            if (idleSounds.Count > 0 && currentState == EnemyState.Wander) // Pouze pokud je ve Wander
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
