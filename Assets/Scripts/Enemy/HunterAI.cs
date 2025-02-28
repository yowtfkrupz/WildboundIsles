using UnityEngine;

public class HunterAI : EnemyAI
{
    [Header("Hunter Settings")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileSpeed = 20f;
    public AudioClip shootSound;
    public AudioSource audioSource;

    protected override void Attack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;

            animator.SetTrigger("Attack"); // Spustí animaci útoku

            if (projectilePrefab != null && projectileSpawnPoint != null)
            {
                GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
                Vector3 direction = (player.position - projectileSpawnPoint.position).normalized;

                Rigidbody rb = projectile.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = direction * projectileSpeed;
                }
                else
                {
                    Debug.LogError("Projectile prefab is missing Rigidbody!");
                    Destroy(projectile);
                    return;
                }

                Collider projectileCollider = projectile.GetComponent<Collider>();
                if (projectileCollider != null)
                {
                    projectileCollider.isTrigger = true;
                }
                else
                {
                    Debug.LogError("Projectile prefab is missing Collider!");
                    Destroy(projectile);
                    return;
                }

                var behavior = projectile.AddComponent<Gemr>();
                behavior.damage = this.damage;

                if (audioSource != null && shootSound != null)
                {
                    audioSource.PlayOneShot(shootSound);
                }
                else
                {
                    Debug.LogWarning("AudioSource or ShootSound is not set in HunterAI.");
                }
            }
            else
            {
                Debug.LogWarning("ProjectilePrefab or ProjectileSpawnPoint is not set in HunterAI.");
            }
        }
    }
}
