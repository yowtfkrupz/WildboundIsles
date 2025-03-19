using UnityEngine;

[CreateAssetMenu(menuName = "Smelting Recipe")]
public class Smelting : ScriptableObject
{
    public InventoryItemData inputItem;
    public InventoryItemData outputItem;
    public float smeltTime;
}