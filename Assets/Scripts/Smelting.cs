using UnityEngine;

[CreateAssetMenu(menuName = "Smelting Recipe")]
public class Smelting : ScriptableObject
{
    public InventoryItemData inputItem;  // Surovina (napø. železná ruda)
    public InventoryItemData outputItem; // Výstup (napø. železný ingot)
    public float smeltTime;              // Èas potøebný k vypálení
}