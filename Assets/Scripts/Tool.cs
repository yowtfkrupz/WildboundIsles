using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : MonoBehaviour
{
    [SerializeField] private Inventory _inventory;
    [SerializeField] public string toolName;  
    [SerializeField] public float damage;   
    [SerializeField] public int tier;      
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask resourceLayer;
    [SerializeField] private GameObject hitEffectPrefab;

    [SerializeField] private bool canMineOre;
    [SerializeField] private bool canChopTree;

    public float lifesteal;

    [SerializeField] private AudioClip stoneHit;
    [SerializeField] private AudioClip woodHit;
    [SerializeField] private AudioClip toolSwing;
    private AudioSource audioSource;

    private Animator _animator;

    private Player player;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false; 
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _animator.Play("ItemAttack");

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, interactionRange))
            {
                SpawnHitEffect(hit.point, hit.normal);

                if (hit.collider.CompareTag("ResourceOre") && canMineOre)
                {
                    Debug.Log("Ore mined!");
                    PlaySound(stoneHit);
                }
                else if (hit.collider.CompareTag("ResourceTree") && canChopTree)
                {
                    Debug.Log("Tree chopped!");
                    PlaySound(woodHit);
                }
                else
                {
                    PlaySound(toolSwing);
                }

                if (hit.collider.CompareTag("Enemy"))
                {
                    EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();
                    if (enemy != null)
                    {
                        enemy.HurtEnemy(damage);
                        PlaySound(toolSwing);
                        ApplyLifesteal(lifesteal);
                    }
                }

                Resource resource = hit.collider.GetComponent<Resource>();
                if (resource != null)
                {
                    if (tier >= resource.RequiredTier)
                    {
                        resource.Harvest(resource, damage);
                        _inventory.DurabilityDamage();
                    }
                    else
                    {
                        Debug.Log("Tier of this tool is too low for this resource!");
                    }
                }
            }
            else
            {
                PlaySound(toolSwing);
            }
        }
    }


    private void SpawnHitEffect(Vector3 position, Vector3 normal)
    {
        if (hitEffectPrefab != null)
        {
            Debug.Log($"Spawning hit effect at {position}");
            Instantiate(hitEffectPrefab, position, Quaternion.LookRotation(normal));
        }
        else
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = position;
            sphere.transform.localScale = Vector3.one * 0.1f;
            Destroy(sphere, 2f);
            Debug.LogWarning("Using test sphere because hitEffectPrefab is not assigned!");
        }
    }
    public void UpdateToolDamage(float damageMultiplier)
    {
        damage *= damageMultiplier;
    }
    private void ApplyLifesteal(float lifesteal)
    {
        float lifestealAmount = damage * lifesteal;
        player.currentHealth += lifestealAmount;

        if (player.currentHealth > player.maxHealth)
        {
            player.currentHealth = player.maxHealth;
        }

        Debug.Log($"Lifesteal applied: {lifestealAmount}, Current Health: {player.currentHealth}");
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
