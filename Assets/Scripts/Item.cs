using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public InventoryItemData MyItem;
    public int Amount;
    public int Durability;
    public Animator _anim;

    void Awake()
    {
        _anim = GetComponent<Animator>();
    }
    public void PlayDestroyAnim()
    {
        _anim.Play("Disappear");
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
    }
    void DestroyMe()
    {
        Destroy(gameObject);
    }
}
