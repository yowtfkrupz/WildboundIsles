using UnityEngine;

public class ConsumableItem : MonoBehaviour
{
    [SerializeField] private Inventory inventory; // Reference na inventáø
    [SerializeField] private Player player;       // Reference na hráèe

    // Efekty konzumovatelného pøedmìtu
    [Header("Consumable Effects")]
    [SerializeField] private int healthRestore = 0;           // Obnovení zdraví
    [SerializeField] private int staminaRestore = 0;          // Obnovení staminy
    [SerializeField] private float damageBoostMultiplier = 1; // Násobitel damage
    [SerializeField] private float damageBoostDuration = 5f;  // Trvání buffu

    private Animator _animator;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    void Update()
    {
        // Použití pøedmìtu pøi levém kliknutí myši
        if (Input.GetMouseButtonDown(0))
        {
            _animator.Play("ItemConsume");
            UseConsumable();
        }
    }

    private void UseConsumable()
    {
        // Získání aktuálního slotu v inventáøi
        Slot currentSlot = inventory.GetCurrentSlot();

        // Pokud slot není prázdný a obsahuje pøedmìt
        if (currentSlot != null && currentSlot.Item != null)
        {
            // Aplikace efektù pøedmìtu

            ApplyEffects();

            // Odebrání jednoho kusu z aktuálního slotu
            inventory.RemoveHandsItem(1);
        }
    }

    private void ApplyEffects()
    {
        // Obnovení zdraví hráèe
        if (healthRestore > 0)
        {
            player.currentHealth = Mathf.Clamp(player.currentHealth + healthRestore, 0, player.maxHealth);
            Debug.Log($"Zdraví obnoveno o {healthRestore}. Aktuální zdraví: {player.currentHealth}");
        }

        // Obnovení staminy hráèe
        if (staminaRestore > 0)
        {
            player.currentStamina = Mathf.Clamp(player.currentStamina + staminaRestore, 0, player.maxStamina);
            Debug.Log($"Stamina obnovena o {staminaRestore}. Aktuální stamina: {player.currentStamina}");
        }

        // Zvýšení damage aktuálního nástroje
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
        // Získání aktuálního slotu a kontrola, zda má vybavený nástroj
        Slot currentSlot = inventory.GetCurrentSlot();
        if (currentSlot != null && currentSlot.Item != null)
        {
            // Vyhledání `Tool` na aktivním first-person modelu
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

        // Aplikace damage boostu
        tool.UpdateToolDamage(damageBoostMultiplier);
        Debug.Log($"Damage boost aktivován! Násobitel: {damageBoostMultiplier} po dobu {damageBoostDuration} sekund.");

        // Poèkání na konec boostu
        yield return new WaitForSeconds(damageBoostDuration);

        // Resetování damage na pùvodní hodnotu
        tool.damage = originalDamage;
        Debug.Log("Damage boost vypršel.");
    }
}
