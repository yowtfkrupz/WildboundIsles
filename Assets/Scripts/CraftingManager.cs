using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    [SerializeField] private Inventory _inventory; // Odkaz na inventáø
    [SerializeField] private List<Crafting> _craftingRecipes; // Seznam všech receptù

    // Metoda pro kontrolu, zda lze daný recept vyrobit
    public bool CanCraft(Crafting recipe)
    {
        return HasRequiredItems(recipe);
    }

    // Metoda pro pokus o vytvoøení pøedmìtu podle receptu
    public bool TryCraft(Crafting recipe)
    {
        if (!HasRequiredItems(recipe))
        {
            Debug.Log("Nedostatek správných surovin!");
            return false;
        }

        RemoveRequiredItems(recipe);
        _inventory.AddItem(recipe.resultItem, recipe.resultAmount, recipe.resultItem.Durability);

        Debug.Log($"Vytvoøil jsi: {recipe.resultItem.Name} x{recipe.resultAmount}");
        return true;
    }

    // Ovìøí, zda hráè má správný druh i množství všech požadovaných surovin
    private bool HasRequiredItems(Crafting recipe)
    {
        foreach (var requirement in recipe.requiredMaterials)
        {
            int itemCount = _inventory.GetItemCount(requirement.requiredItem);
            if (itemCount < requirement.requiredAmount)
            {
                Debug.Log($"Chybí surovina: {requirement.requiredItem.Name}, požadováno: {requirement.requiredAmount}, má: {itemCount}");
                return false;
            }
        }
        return true;
    }

    // Odebírá pouze správné druhy surovin ve správném množství
    private void RemoveRequiredItems(Crafting recipe)
    {
        foreach (var requirement in recipe.requiredMaterials)
        {
            int remainingToRemove = requirement.requiredAmount;
            List<Slot> allSlots = new List<Slot>(_inventory._hotbarSlots);
            allSlots.AddRange(_inventory._inventorySlots);

            foreach (var slot in allSlots)
            {
                if (slot.Item != null && slot.Item == requirement.requiredItem)
                {
                    if (slot.Amount >= remainingToRemove)
                    {
                        slot.RemoveItem(remainingToRemove);
                        break;
                    }
                    else
                    {
                        remainingToRemove -= slot.Amount;
                        slot.RemoveItem(slot.Amount);
                    }
                }
            }
        }
    }
}
