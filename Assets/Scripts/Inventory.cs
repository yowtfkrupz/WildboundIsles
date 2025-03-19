using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Pøidáno pro práci s TextMeshPro

public class Inventory : MonoBehaviour
{
    [SerializeField] public Slot[] _hotbarSlots;
    [SerializeField] public Slot[] _inventorySlots;

    [SerializeField] ItemModels[] _itemModels;

    [SerializeField] GameObject _inventoryTab;
    [SerializeField] PlayerCam _playerCam;
    [SerializeField] Animator _crosshair;

    [Header("Raycast")]
    [SerializeField] float _rayDistance;
    [SerializeField] LayerMask _rayMask;
    [Header("Throwing")]
    [SerializeField] Transform _throwPoint;
    [SerializeField] float _throwForce;

    [Header("Gold")] // Pøidáno
    [SerializeField] private int gold = 0; // Poèet zlatých mincí
    [SerializeField] private TextMeshProUGUI goldText; // Odkaz na UI text pro zobrazení zlatých mincí

    int _currentSlot;

    void Start()
    {
        UpdateSlots();
        UpdateGoldUI(); // Pøidáno: Aktualizace UI na zaèátku
    }

    // Update is called once per frame
    void Update()
    {
        RaycastToItem();
        int prevSlot = _currentSlot;

        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            _currentSlot++;
            if (_currentSlot > _hotbarSlots.Length - 1)
            {
                _currentSlot = 0;
            }
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            _currentSlot--;
            if (_currentSlot < 0)
            {
                _currentSlot = _hotbarSlots.Length - 1;
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _currentSlot = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _currentSlot = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _currentSlot = 2;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            _currentSlot = 3;
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            _currentSlot = 4;
        }

        if (prevSlot != _currentSlot)
        {
            UpdateSlots();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _inventoryTab.SetActive(!_inventoryTab.activeInHierarchy);
            _playerCam.LockMovement(_inventoryTab.activeInHierarchy);
        }

        if (Input.GetKeyDown(KeyCode.G) && _hotbarSlots[_currentSlot].Item != null)
        {
            ThrowItems(_hotbarSlots[_currentSlot].Item, 1, _hotbarSlots[_currentSlot].Durability);
            RemoveHandsItem(1);
        }
    }

    public void UpdateSlots()
    {
        for (int i = 0; i < _hotbarSlots.Length; i++)
        {
            if (_currentSlot == i)
            {
                _hotbarSlots[i].Select();
                for (int m = 0; m < _itemModels.Length; m++)
                {
                    if (_itemModels[m].Item == _hotbarSlots[i].Item)
                    {
                        _itemModels[m].model.SetActive(true);
                    }
                    else
                    {
                        _itemModels[m].model.SetActive(false);
                    }
                }
            }
            else
            {
                _hotbarSlots[i].Deselect();
            }
        }
    }

    void RaycastToItem()
    {
        RaycastHit hit;
        bool highlight = false;
        if (Physics.Raycast(_playerCam.transform.position, _playerCam.transform.forward, out hit, _rayDistance, _rayMask))
        {
            if (hit.transform.tag == "Item")
            {
                highlight = true;
                if (Input.GetMouseButton(1))
                {
                    AddItem(hit.transform.GetComponent<Item>().MyItem, hit.transform.GetComponent<Item>().Amount, hit.transform.GetComponent<Item>().Durability);
                    hit.transform.GetComponent<Item>().PlayDestroyAnim();
                    hit.transform.GetComponent<Item>()._anim.enabled = true;
                }
            }
        }
        HighlightCrosshair(highlight);
    }
    public Slot GetCurrentSlot()
    {
        if (_currentSlot >= 0 && _currentSlot < _hotbarSlots.Length)
        {
            return _hotbarSlots[_currentSlot];
        }
        return null;
    }

    void HighlightCrosshair(bool highlight)
    {
        if (highlight)
        {
            _crosshair.Play("Interact");
        }
        else
        {
            _crosshair.Play("NoInteract");
        }
    }

    public void AddItem(InventoryItemData item, int amount, int durability)
    {
        List<Slot> allSlots = new List<Slot>();
        allSlots.AddRange(_hotbarSlots);
        allSlots.AddRange(_inventorySlots);
        for (int i = 0; i < allSlots.Count; i++)
        {
            if (allSlots[i].Item == item && allSlots[i].Amount < item.StackSize)
            {
                if (allSlots[i].Amount + amount <= item.StackSize)
                {
                    allSlots[i].AddItem(item, amount, durability);
                    UpdateSlots();
                    return;
                }
                else
                {
                    amount += allSlots[i].Amount - item.StackSize;
                    allSlots[i].AddItem(item, item.StackSize - allSlots[i].Amount, durability);
                }
            }
        }
        for (int i = 0; i < allSlots.Count; i++)
        {
            if (allSlots[i].Item == null)
            {
                if (allSlots[i].Amount + amount <= item.StackSize)
                {
                    allSlots[i].AddItem(item, amount, durability);
                    UpdateSlots();
                    return;
                }
                else
                {
                    amount += allSlots[i].Amount - item.StackSize;
                    allSlots[i].AddItem(item, item.StackSize - allSlots[i].Amount, durability);
                }
            }
        }
    }

    public void RemoveHandsItem(int amount)
    {
        if (_hotbarSlots[_currentSlot].Item != null)
        {
            _hotbarSlots[_currentSlot].RemoveItem(amount);
            UpdateSlots();
        }
    }
    public bool RemoveItemByID(int id, int amount)
    {
        List<Slot> allSlots = new List<Slot>();
        allSlots.AddRange(_hotbarSlots);
        allSlots.AddRange(_inventorySlots);

        foreach (var slot in allSlots)
        {
            if (slot.Item != null && slot.Item.ID == id)
            {
                if (slot.Amount >= amount)
                {
                    slot.RemoveItem(amount);
                    UpdateSlots();
                    return true;
                }
                else
                {
                    amount -= slot.Amount;
                    slot.RemoveItem(slot.Amount);
                }
            }
        }

        Debug.LogWarning("Not enough items to remove.");
        return false;
    }

    public void ThrowItems(InventoryItemData item, int amount, int durability)
    {
        for (int i = 0; i < amount; i++)
        {
            Rigidbody rb = Instantiate(item.Prefab, _throwPoint.position, _throwPoint.rotation).GetComponent<Rigidbody>();
            rb.AddForce(_throwPoint.forward * _throwForce * i, ForceMode.Impulse);
            rb.GetComponent<Item>().Durability = durability;
            rb.GetComponent<Item>()._anim.enabled = false;
        }
    }
    public int GetItemCount(InventoryItemData itemData)
    {
        int totalAmount = 0;
        List<Slot> allSlots = new List<Slot>(_hotbarSlots);
        allSlots.AddRange(_inventorySlots);

        foreach (var slot in allSlots)
        {
            if (slot.Item != null && slot.Item == itemData)
            {
                totalAmount += slot.Amount;
            }
        }
        return totalAmount;
    }


    public void DurabilityDamage()
    {
        _hotbarSlots[_currentSlot].DecreaseDurability();
        if (_hotbarSlots[_currentSlot].Durability == 0)
        {
            RemoveHandsItem(1);
        }
    }

    // Pøidáno: Pøístup k zlatým mincím
    public int GetGold()
    {
        return gold;
    }

    public void AddGold(int amount)
    {
        gold += amount;
        UpdateGoldUI();
    }

    public void RemoveGold(int amount)
    {
        gold -= amount;
        if (gold < 0) gold = 0;
        UpdateGoldUI();
    }

    private void UpdateGoldUI() // Pøidáno
    {
        if (goldText != null)
        {
            goldText.text = $"{gold}";
        }
    }

    [System.Serializable]
    class ItemModels
    {
        public InventoryItemData Item;
        public GameObject model;
    }
}
