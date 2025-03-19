using UnityEngine;

public class PerkPickup : MonoBehaviour
{
    public Perk perk;
    private bool playerInRange = false;
    private PerkChest parentChest;

    void Start()
    {
        parentChest = GetComponentInParent<PerkChest>();
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            PerkManager perkManager = FindObjectOfType<PerkManager>();
            if (perkManager != null && perk != null)
            {
                perkManager.ApplyPerk(perk);
                Debug.Log($"Player collected perk: {perk.perkName}");

                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
