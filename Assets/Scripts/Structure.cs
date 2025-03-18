using UnityEngine;
using UnityEngine.UI;

public class Structure : MonoBehaviour
{
    [Header("Structure Settings")]
    [SerializeField] private GameObject ghostModelPrefab;
    [SerializeField] private GameObject realModelPrefab;
    [SerializeField] private LayerMask placementMask;
    [SerializeField] private Inventory playerInventory;
    [SerializeField] private InventoryItemData itemData;
    [SerializeField] private float maxPlacementDistance = 5f;

    [Header("UI Settings")]
    [SerializeField] private GameObject placementUIText;

    private GameObject ghostModel;
    private Transform playerCamera;
    private float rotationAngle = 0f;
    private bool isPlacing = false;

    private void Start()
    {
        playerCamera = Camera.main.transform;
        Debug.Log("Structure script started.");

        if (gameObject.activeInHierarchy)
        {
            ActivateGhostMode();
        }
    }

    private void Update()
    {
        if (isPlacing)
        {
            if (playerInventory.GetCurrentSlot().Item == null || playerInventory.GetCurrentSlot().Item != itemData)
            {
                CancelPlacing();
                return;
            }

            UpdateGhostPosition();
            HandleRotation();
            HandlePlacement();
        }
    }

    public void ActivateGhostMode()
    {
        Debug.Log("ActivateGhostMode called.");
        if (!isPlacing && playerInventory.GetCurrentSlot().Item != null && playerInventory.GetCurrentSlot().Item == itemData)
        {
            Debug.Log("Item matches itemData, starting ghost mode...");
            StartPlacing();
        }
        else
        {
            Debug.LogWarning("Cannot activate ghost mode. Either item is missing or does not match itemData.");
        }
    }

    public void StartPlacing()
    {
        if (playerInventory.GetItemCount(itemData) > 0)
        {
            isPlacing = true;
            ghostModel = Instantiate(ghostModelPrefab);
            ghostModel.GetComponent<Collider>().enabled = false;
            Debug.Log("Ghost model created at: " + ghostModel.transform.position);

            if (placementUIText != null)
            {
                placementUIText.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("You don't have the required item to place this structure.");
        }
    }

    private void UpdateGhostPosition()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxPlacementDistance, placementMask))
        {
            ghostModel.transform.position = hit.point;
            ghostModel.transform.rotation = Quaternion.Euler(0, rotationAngle, 0);
        }
        else
        {
            Vector3 fallbackPosition = playerCamera.position + playerCamera.forward * maxPlacementDistance;
            fallbackPosition.y = GetTerrainHeightAtPosition(fallbackPosition);
            ghostModel.transform.position = fallbackPosition;
            ghostModel.transform.rotation = Quaternion.Euler(0, rotationAngle, 0);
        }
    }

    private void HandleRotation()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            rotationAngle += 45f;
            if (rotationAngle >= 360f)
            {
                rotationAngle = 0f;
            }
        }
    }

    private void HandlePlacement()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PlaceStructure();
        }
    }

    private void PlaceStructure()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxPlacementDistance, placementMask))
        {
            Instantiate(realModelPrefab, ghostModel.transform.position, ghostModel.transform.rotation);
            Debug.Log("Structure placed!");

            if (playerInventory.RemoveItemByID(itemData.ID, 1))
            {
                CancelPlacing();
            }
            else
            {
                Debug.LogWarning("Failed to remove item from inventory.");
            }
        }
        else
        {
            Debug.LogError("Invalid placement location.");
        }
    }

    private void CancelPlacing()
    {
        isPlacing = false;
        if (ghostModel != null)
        {
            Destroy(ghostModel);
        }

        if (placementUIText != null)
        {
            placementUIText.SetActive(false);
        }
    }

    private float GetTerrainHeightAtPosition(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * 100f, Vector3.down, out hit, Mathf.Infinity, placementMask))
        {
            return hit.point.y;
        }
        return position.y;
    }
}
