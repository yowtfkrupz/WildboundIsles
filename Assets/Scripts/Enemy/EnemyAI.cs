using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine;

public enum EnemyState { Wander, Chase, Attack }

public class EnemyAI : MonoBehaviour
{
    public EnemyState currentState;
    protected Transform player;
    protected Animator animator;

    [Header("Enemy Settings")]
    public float maxHealth = 100f;
    public float health;
    public float damage;
    public float chaseRange;
    public float wanderSpeed;
    public float chaseSpeed;
    public float attackRange;
    public float attackCooldown;
    public float wanderRadius = 15f;
    public float lastAttackTime;
    private NavMeshAgent agent;
    private Vector3 wanderTarget;
    private float wanderTimer = 0f;
    public float wanderInterval = 3f;

    [Header("Healthbar")]
    [SerializeField] public Canvas healthCanvas; // Odkaz na Canvas s healthbarem
    public Image healthBarImage;

    [Header("Drop")]
    public GameObject dropPrefab;

    [Header("Boss Settings")]
    public bool isBoss = false;
    [SerializeField] public GameObject bossHealthBarUI;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        lastAttackTime = Time.time;
        health = maxHealth;

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure the player object has the tag 'Player'.");
        }

        // Automatické pøiøazení health baru pro bosse
        if (isBoss)
        {
            string bossName = gameObject.name; // Jméno prefabu nebo instance bosse
            bossHealthBarUI = GameObject.Find($"{bossName}Bar");
            if (bossHealthBarUI == null)
            {
                Debug.LogError($"HealthBar for boss '{bossName}' not found in the scene!");
            }
            else
            {
                bossHealthBarUI.SetActive(true);
            }
        }
        else if (healthCanvas != null)
        {
            healthCanvas.gameObject.SetActive(true);
        }
    }


    void Update()
    {
        if (player == null) return;

        switch (currentState)
        {
            case EnemyState.Wander:
                Wander();
                if (Vector3.Distance(transform.position, player.position) < chaseRange)
                    currentState = EnemyState.Chase;
                break;

            case EnemyState.Chase:
                Chase();
                if (Vector3.Distance(transform.position, player.position) < attackRange)
                    currentState = EnemyState.Attack;
                else if (Vector3.Distance(transform.position, player.position) > chaseRange)
                    currentState = EnemyState.Wander;
                break;

            case EnemyState.Attack:
                Attack();
                if (Vector3.Distance(transform.position, player.position) > attackRange)
                    currentState = EnemyState.Chase;
                break;
        }
    }
    public void UpdateHealthBar()
    {
        if (isBoss && bossHealthBarUI != null)
        {
            Image bossHealthImage = bossHealthBarUI.GetComponentInChildren<Image>();
            if (bossHealthImage != null)
            {
                bossHealthImage.fillAmount = health / maxHealth;
            }
        }
        else if (healthBarImage != null)
        {
            healthBarImage.fillAmount = health / maxHealth;
        }
        else
        {
            Debug.LogWarning($"HealthBarImage is missing for {gameObject.name}");
        }
    }

    public virtual void HurtEnemy(float damage)
    {
        if (animator != null)
        {
            animator.SetTrigger("IsHit"); // Animace po zásahu
        }
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthBar();

        if (health <= 0)
        {
            DieEnemy();
        }
    }

    protected virtual void DieEnemy()
    {
        if (dropPrefab != null)
        {
            Instantiate(dropPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("DropPrefab is not assigned. No drop will spawn.");
        }

        Destroy(gameObject);
    }

    protected virtual void Attack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;

            animator.SetTrigger("Attack");

            Player playerScript = player.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damage);
                Debug.Log("Enemy attacked the player!");
            }
        }
    }

    protected virtual void SpecialAbility()
    {
        // Special ability logic to be overridden
    }

    protected void Wander()
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
            animator.SetBool("IsWalking", true);

        wanderTimer += Time.deltaTime;
        if (wanderTimer >= wanderInterval || Vector3.Distance(transform.position, wanderTarget) < 1f)
        {
            SetNewWanderTarget();
            wanderTimer = 0f;
        }

        agent.SetDestination(wanderTarget);
        agent.speed = wanderSpeed;
    }

    protected void SetNewWanderTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            wanderTarget = hit.position;
        }
    }

    protected virtual void Chase()
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
            animator.SetBool("IsRunning", true);

        agent.SetDestination(player.position);
        agent.speed = chaseSpeed;
    }
}
