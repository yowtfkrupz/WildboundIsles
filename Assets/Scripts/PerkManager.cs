using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PerkManager : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject perkPanel;
    public GameObject perkIconPrefab;
    public GameObject tooltip;
    public TextMeshProUGUI tooltipTitle;
    public TextMeshProUGUI tooltipDescription;

    private List<Perk> activePerks = new List<Perk>();
    private Player player;
    private Tool tool;

    void Start()
    {
        player = GetComponent<Player>();
        if (tooltip != null) tooltip.SetActive(false);
    }

    public void ApplyPerk(Perk perk)
    {

        if (!activePerks.Contains(perk))
        {
            activePerks.Add(perk);
            AddPerkToUI(perk);
            ApplyPerkEffect(perk);
            Debug.Log($"Perk {perk.perkName} applied!");
        }
        else
        {
            Debug.Log($"Player already has perk {perk.perkName}");
        }
    }

    private void AddPerkToUI(Perk perk)
    {
        GameObject newIcon = Instantiate(perkIconPrefab, perkPanel.transform);
        Image iconImage = newIcon.GetComponent<Image>();
        if (iconImage != null)
        {
            iconImage.sprite = perk.icon;
        }
        else
        {
            Debug.LogWarning("PerkIconPrefab does not have an Image component!");
        }

        EventTrigger trigger = newIcon.AddComponent<EventTrigger>();
        AddEventTrigger(trigger, EventTriggerType.PointerEnter, (data) => ShowTooltip(perk, Input.mousePosition));
        AddEventTrigger(trigger, EventTriggerType.PointerExit, (data) => HideTooltip());
    }
    private void ShowTooltip(Perk perk, Vector3 position)
    {
        if (tooltip != null)
        {
            tooltip.SetActive(true);

            Vector3 offset = new Vector3(15f, -15f, 0f);
            tooltip.transform.position = position + offset;

            tooltipTitle.text = perk.perkName;
            tooltipDescription.text = perk.description;
        }
    }


    private void HideTooltip()
    {
        if (tooltip != null)
        {
            tooltip.SetActive(false);
        }
    }
    private void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

    private void ApplyPerkEffect(Perk perk)
    {
        if (player == null)
        {
            Debug.LogError("Player component not found!");
            return;
        }

        switch (perk.effectType)
        {
            case PerkEffectType.InfiniteStamina:
                player.hasInfiniteStamina = true;
                break;
            case PerkEffectType.MovementSpeedBoost:
                player.walkSpeed = player.walkSpeed * perk.movementSpeedBoost;
                player.sprintSpeed = player.sprintSpeed * perk.movementSpeedBoost;
                break;
            case PerkEffectType.JumpBoost:
                player.jumpForce = player.jumpForce * perk.jumpBoost;
                break;
            case PerkEffectType.DamageBoost:
                if (tool != null)
                {
                    tool.UpdateToolDamage(perk.damageBoost);
                }
                break;
            case PerkEffectType.DamageBoostOnLowHealth:
                if (player.currentHealth <= perk.lowHealthThreshold)
                {
                    if (tool != null)
                    {
                        tool.UpdateToolDamage(perk.damageBoost);
                    }
                }
                break;
            case PerkEffectType.Lifesteal:
                if (tool != null)
                {
                    tool.lifesteal = perk.lifesteal;
                }
                break;
            default:
                Debug.LogWarning($"Effect for perk {perk.perkName} not defined.");
                break;
        }
    }
    private void ApplyLifesteal(float damageDealt, Perk perk)
    {
        float lifestealAmount = damageDealt * perk.lifesteal;

        player.currentHealth += lifestealAmount;

        if (player.currentHealth > player.maxHealth)
        {
            player.currentHealth = player.maxHealth;
        }

        Debug.Log($"Lifesteal applied: {lifestealAmount}, Current Health: {player.currentHealth}");
    }
}