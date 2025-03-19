using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Portal : MonoBehaviour
{
    [SerializeField] private List<InventoryItemData> requiredBossItems;
    [SerializeField] private List<GameObject> bossItemObjects;
    [SerializeField] private GameObject portalEffect;
    [SerializeField] private GameObject winScreenUI;

    private HashSet<int> placedItems = new HashSet<int>();
    private Inventory playerInventory;
    private bool portalActivated = false;

    void Start()
    {
        playerInventory = FindObjectOfType<Inventory>();

        portalEffect.SetActive(false);
        winScreenUI.SetActive(false);

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

        for (int i = 0; i < requiredBossItems.Count; i++)
        {
            if (heldItem == requiredBossItems[i] && !placedItems.Contains(heldItem.ID))
            {
                if (playerInventory.RemoveItemByID(heldItem.ID, 1))
                {
                    bossItemObjects[i].SetActive(true);
                    placedItems.Add(heldItem.ID);
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
            portalEffect.SetActive(true);
            portalActivated = true;
            Debug.Log("Portál aktivován!");
            ShowWinScreen();
        }
    }

    void ShowWinScreen()
    {
        winScreenUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
