using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentinelAI : EnemyAI
{
    private bool isHealing = false;
    private bool hasHealed = false;

    protected override void SpecialAbility()
    {
        if (!hasHealed)
        {
            hasHealed = true;
            StartCoroutine(HealSequence());
        }
    }

    private IEnumerator HealSequence()
    {
        Debug.Log("Sentinel activates Unyielding Resolve!");
        isHealing = true;
        agent.isStopped = true;
        animator.SetTrigger("Heal");

        yield return new WaitForSeconds(3f);

        health += 75f;
        UpdateHealthBar();
        Debug.Log("Sentinel has healed!");

        isHealing = false;
        agent.isStopped = false;
    }

    public override void HurtEnemy(float damage)
    {
        if (!isHealing)
        {
            base.HurtEnemy(damage);
        }
        else
        {
            Debug.Log("Sentinel is healing and cannot be harmed!");
        }
    }

    protected override void Attack()
    {
        if (!isHealing)
        {
            base.Attack();
        }
    }
}
