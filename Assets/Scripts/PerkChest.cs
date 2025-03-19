using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerkChest : MonoBehaviour
{
    [Header("Chest Settings")]
    public string chestName;
    public string quality;
    public int price;

    [Header("Perk Pools")]
    public List<GameObject> commonPerks;
    public List<GameObject> uncommonPerks;
    public List<GameObject> rarePerks;
    public List<GameObject> epicPerks;
    public List<GameObject> legendaryPerks;
    public List<GameObject> mythicalPerks;

    private bool isOpened = false;
    private bool playerInRange = false;
    private Animator animator;
    private Inventory playerInventory;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (playerInRange && !isOpened && Input.GetKeyDown(KeyCode.E))
        {
            if (playerInventory != null && playerInventory.GetGold() >= price)
            {
                OpenChest();
                playerInventory.RemoveGold(price);
            }
            else
            {
                Debug.Log("Not enough gold to open the chest.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerInventory = other.GetComponentInParent<Inventory>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerInventory = null;
        }
    }

    private void OpenChest()
    {
        isOpened = true;
        if (animator != null)
        {
            animator.SetTrigger("OpenChest");
        }
        SpawnPerk();
    }

    private void SpawnPerk()
    {
        GameObject perkPrefab = ChoosePerk();
        if (perkPrefab != null)
        {
            GameObject spawnedPerk = Instantiate(perkPrefab, transform.position + Vector3.up, Quaternion.identity);
            spawnedPerk.transform.SetParent(transform);
            AudioSource perkAudio = spawnedPerk.GetComponent<AudioSource>();
            if (perkAudio != null)
            {
                perkAudio.Play();
            }
            else
            {
                Debug.LogWarning("Spawned perk has no AudioSource component!");
            }
        }
    }

    private GameObject ChoosePerk()
    {
        List<GameObject> chosenPool = commonPerks;
        switch (quality.ToLower())
        {
            case "uncommon":
                chosenPool = uncommonPerks;
                break;
            case "rare":
                chosenPool = rarePerks;
                break;
            case "epic":
                chosenPool = epicPerks;
                break;
            case "legendary":
                chosenPool = legendaryPerks;
                break;
            case "mythical":
                chosenPool = mythicalPerks;
                break;
        }

        if (chosenPool != null && chosenPool.Count > 0)
        {
            int randomIndex = Random.Range(0, chosenPool.Count);
            return chosenPool[randomIndex];
        }

        Debug.LogWarning("No perks available in this pool!");
        return null;
    }
}
