using System.Collections;
using UnityEngine;

public class RibCage : MonoBehaviour
{
    public float damage;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void TriggerAppear()
    {
        animator.SetTrigger("Appear");
        StartCoroutine(DisappearSequence());
    }

    private IEnumerator DisappearSequence()
    {
        yield return new WaitForSeconds(7f); // 4s Appear + 3s drûenÌ

        animator.SetTrigger("Disappear");

        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player playerScript = other.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damage);
                Debug.Log($"RibCage zas·hlo hr·Ëe! Hr·Ë dostal {damage} damage.");
            }
        }
    }
}
