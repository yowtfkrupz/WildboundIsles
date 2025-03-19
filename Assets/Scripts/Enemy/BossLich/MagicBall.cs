using UnityEngine;

public class MagicBall : MonoBehaviour
{
    public float damage;
    public Collider lichCollider;

    private void Start()
    {
        if (lichCollider != null)
        {
            Collider projectileCollider = GetComponent<Collider>();
            if (projectileCollider != null)
            {
                Physics.IgnoreCollision(projectileCollider, lichCollider);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player playerScript = other.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damage);
                Debug.Log($"MagicBall zas·hl hr·Ëe! Hr·Ë dostal {damage} damage.");
            }
        }
        Destroy(gameObject);
    }
}
