using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SmeltingStation : MonoBehaviour
{
    [Header("Smelting UI")]
    [SerializeField] private GameObject smeltingUI;
    [SerializeField] private PlayerCam playerCam;
    [SerializeField] private MonoBehaviour playerController;
    [SerializeField] private Animator crosshair;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private LayerMask interactMask;

    [Header("UI Slots (UNIKÁTNÍ PRO KAŽDOU PEC)")]
    [SerializeField] private Slot inputSlot;
    [SerializeField] private Slot fuelSlot;
    [SerializeField] private Slot outputSlot;

    [Header("Progress Bars")]
    [SerializeField] private Image burnProgressBar;
    [SerializeField] private Image smeltProgressBar;

    [Header("Smelting Data")]
    [SerializeField] private Smelting[] smeltingRecipes;
    [SerializeField] private InventoryItemData[] fuelItems;
    private HashSet<InventoryItemData> fuelSet;

    [Header("Effects")]
    [SerializeField] private ParticleSystem smeltingEffect;

    private Smelting currentRecipe;
    private bool isSmelting = false;
    private bool isBurning = false;
    private float currentSmeltTime = 0f;
    private float currentBurnTime = 0f;
    private const float burnTime = 10f;
    private bool isUIActive = false;
    private bool playerNearby = false; // Každá pec sleduje, jestli u ní stojí hráè

    private void Start()
    {
        if (smeltingUI != null)
        {
            smeltingUI.SetActive(false);
        }

        fuelSet = new HashSet<InventoryItemData>(fuelItems);
        if (smeltingEffect != null) smeltingEffect.Stop(); // Efekt vypnutý na zaèátku
    }

    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(interactKey))
        {
            ToggleSmeltingUI();
        }

        if (isBurning)
        {
            ProcessBurning();
        }

        if (isSmelting)
        {
            ProcessSmelting();
        }
        else
        {
            TryStartSmelting();
        }
    }

    private void ToggleSmeltingUI()
    {
        isUIActive = !isUIActive;
        smeltingUI.SetActive(isUIActive);

        playerCam?.LockMovement(isUIActive);
        if (playerController != null) playerController.enabled = !isUIActive;
        crosshair?.SetBool("IsCrafting", isUIActive);

        Debug.Log($"Smelting UI {(isUIActive ? "opened" : "closed")}");
    }

    private void TryStartSmelting()
    {
        if (inputSlot.Item == null || fuelSlot.Item == null || fuelSlot.Amount <= 0)
            return;

        currentRecipe = FindRecipe(inputSlot.Item);
        if (currentRecipe == null)
            return;

        if (!fuelSet.Contains(fuelSlot.Item))
        {
            Debug.Log("Tento item nelze použít jako palivo!");
            return;
        }

        if (!isBurning)
        {
            StartBurning();
        }

        isSmelting = true;
        currentSmeltTime = 0f;
        smeltingEffect?.Play(); // Aktivace efektu
    }

    private void StartBurning()
    {
        if (fuelSlot.Amount > 0)
        {
            fuelSlot.RemoveItem(1);
            isBurning = true;
            currentBurnTime = burnTime;
        }
    }

    private void ProcessBurning()
    {
        currentBurnTime -= Time.deltaTime;
        burnProgressBar.fillAmount = currentBurnTime / burnTime;

        if (currentBurnTime <= 0)
        {
            isBurning = false;

            if (fuelSlot.Amount > 0 && fuelSet.Contains(fuelSlot.Item))
            {
                StartBurning();
            }
            else
            {
                isSmelting = false;
                smeltingEffect?.Stop(); // Vypnutí efektu, pokud není palivo
            }
        }
    }

    private void ProcessSmelting()
    {
        if (inputSlot.Item == null || !isBurning)
        {
            isSmelting = false;
            smeltingEffect?.Stop();
            return;
        }

        currentSmeltTime += Time.deltaTime;
        smeltProgressBar.fillAmount = currentSmeltTime / currentRecipe.smeltTime;

        if (currentSmeltTime >= currentRecipe.smeltTime)
        {
            CompleteSmelting();
        }
    }

    private void CompleteSmelting()
    {
        inputSlot.RemoveItem(1);
        outputSlot.AddItem(currentRecipe.outputItem, 1, 0);
        currentSmeltTime = 0f;

        if (inputSlot.Amount <= 0)
        {
            inputSlot.RemoveItem();
            isSmelting = false;
        }
    }

    private Smelting FindRecipe(InventoryItemData input)
    {
        foreach (var recipe in smeltingRecipes)
        {
            if (recipe.inputItem == input)
                return recipe;
        }
        return null;
    }

    // **Oprava více pecí – UI se otevøe jen pro pec, u které hráè stojí**
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;

            // Automaticky zavøeme UI, když hráè odejde
            if (isUIActive)
            {
                ToggleSmeltingUI();
            }
        }
    }
}
