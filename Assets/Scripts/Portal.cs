using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private List<InventoryItemData> requiredBossItems; // Seznam požadovaných itemù
    [SerializeField] private List<GameObject> bossItemObjects; // Odpovídající objekty na portálu
    [SerializeField] private GameObject portalEffect; // Efekt/èásti portálu, které se aktivují po dokonèení
    private HashSet<int> placedItems = new HashSet<int>(); // Sledování již umístìných itemù

    private Inventory playerInventory;

    void Start()
    {
        playerInventory = FindObjectOfType<Inventory>(); // Najde hráèùv inventáø
        portalEffect.SetActive(false); // Portál je na zaèátku neaktivní

        // Skryjeme objekty boss itemù na portálu
        foreach (var obj in bossItemObjects)
        {
            obj.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPlaceBossItem();
        }
    }

    void TryPlaceBossItem()
    {
        var currentSlot = playerInventory.GetCurrentSlot();
        if (currentSlot == null || currentSlot.Item == null) return;

        InventoryItemData heldItem = currentSlot.Item;

        // Ovìøíme, zda hráè drží nìkterý z požadovaných boss itemù
        for (int i = 0; i < requiredBossItems.Count; i++)
        {
            if (heldItem == requiredBossItems[i] && !placedItems.Contains(heldItem.ID))
            {
                if (playerInventory.RemoveItemByID(heldItem.ID, 1))
                {
                    bossItemObjects[i].SetActive(true); // Aktivujeme odpovídající objekt na portálu
                    placedItems.Add(heldItem.ID); // Pøidáme item do seznamu umístìných itemù
                    CheckPortalCompletion();
                }
                return;
            }
        }
    }

    void CheckPortalCompletion()
    {
        if (placedItems.Count == requiredBossItems.Count)
        {
            portalEffect.SetActive(true); // Aktivace portálu po vložení všech itemù
            Debug.Log("Portál aktivován!");
        }
    }
}
