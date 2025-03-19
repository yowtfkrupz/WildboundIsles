using UnityEngine;

[CreateAssetMenu(fileName = "New Perk", menuName = "Perks/Perk")]
public class Perk : ScriptableObject
{
    public string perkName;            
    public string description;        
    public Sprite icon;                
    public PerkRarity rarity;          
    public PerkEffectType effectType;


    public float healthBoost;        
    public float staminaBoost;        
    public float healthRegen;          
    public float staminaRegen;      
    public bool infiniteStamina;       
    public float damageBoost;          
    public float lowHealthThreshold;  
    public float movementSpeedBoost;
    public float jumpBoost;
    public float lifesteal;
    public float saturationBoost;
}

public enum PerkRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary,
    Mythical
}

public enum PerkEffectType
{
    HealthBoost,       
    StaminaBoost,      
    HealthRegen,          
    StaminaRegen,          
    InfiniteStamina,       
    DamageBoost,            
    DamageBoostOnLowHealth,
    MovementSpeedBoost,
    JumpBoost,
    Lifesteal,
    saturationBoost
}
