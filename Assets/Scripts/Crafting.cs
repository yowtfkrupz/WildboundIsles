using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Crafting Recipe")]
public class Crafting : ScriptableObject
{
    public string RecipeName; // Název receptu
    public InventoryItemData resultItem; // Výsledný item
    public int resultAmount; // Poèet vyrobených kusù

    [System.Serializable]
    public struct CraftingRequirement
    {
        public InventoryItemData requiredItem;
        public int requiredAmount;
    }

    public CraftingRequirement[] requiredMaterials; // Seznam surovin pro výrobu
}
