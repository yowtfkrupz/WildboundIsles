using UnityEngine;

public class ConsumableItem : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private Player player;

    [Header("Consumable Effects")]
    [SerializeField] private int healthRestore = 0;         
    [SerializeField] private int staminaRestore = 0;         
    [SerializeField] private float damageBoostMultiplier = 1;
    [SerializeField] private float damageBoostDuration = 5f;

    private Animator _animator;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _animator.Play("ItemConsume");
            UseConsumable();
        }
    }

    private void UseConsumable()
    {
        Slot currentSlot = inventory.GetCurrentSlot();

        if (currentSlot != null && currentSlot.Item != null)
        {
            ApplyEffects();

            inventory.RemoveHandsItem(1);
        }
    }

    private void ApplyEffects()
    {
        if (healthRestore > 0)
        {
            player.currentHealth = Mathf.Clamp(player.currentHealth + healthRestore, 0, player.maxHealth);
            Debug.Log($"Zdraví obnoveno o {healthRestore}. Aktuální zdraví: {player.currentHealth}");
        }
        if (staminaRestore > 0)
        {
            player.currentStamina = Mathf.Clamp(player.currentStamina + staminaRestore, 0, player.maxStamina);
            Debug.Log($"Stamina obnovena o {staminaRestore}. Aktuální stamina: {player.currentStamina}");
        }
        if (damageBoostMultiplier > 1 && damageBoostDuration > 0)
        {
            Tool equippedTool = GetEquippedTool();
            if (equippedTool != null)
            {
                StartCoroutine(ApplyDamageBoost(equippedTool));
            }
        }
    }

    private Tool GetEquippedTool()
    {
        Slot currentSlot = inventory.GetCurrentSlot();
        if (currentSlot != null && currentSlot.Item != null)
        {
            GameObject toolModel = currentSlot.Item.Prefab;
            if (toolModel != null)
            {
                Tool tool = toolModel.GetComponent<Tool>();
                if (tool != null)
                {
                    return tool;
                }
            }
        }

        Debug.LogWarning("Žádný vybavený nástroj nebyl nalezen!");
        return null;
    }

    private System.Collections.IEnumerator ApplyDamageBoost(Tool tool)
    {
        float originalDamage = tool.damage;

        tool.UpdateToolDamage(damageBoostMultiplier);
        Debug.Log($"Damage boost aktivován! Násobitel: {damageBoostMultiplier} po dobu {damageBoostDuration} sekund.");

        yield return new WaitForSeconds(damageBoostDuration);

        tool.damage = originalDamage;
        Debug.Log("Damage boost vypršel.");
    }
}
