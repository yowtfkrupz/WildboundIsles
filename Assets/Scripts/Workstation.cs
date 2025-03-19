using UnityEngine;

public class Workstation : MonoBehaviour
{
    [Header("Crafting UI")]
    [SerializeField] private GameObject craftingUI;
    [SerializeField] private PlayerCam playerCam;
    [SerializeField] private MonoBehaviour playerController;
    [SerializeField] private Animator crosshair;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private LayerMask interactMask;

    private Camera playerCamera;
    private bool isCraftingActive = false;

    private void Start()
    {
        playerCamera = Camera.main;
        if (craftingUI != null)
        {
            craftingUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactMask))
        {
            if (hit.collider.gameObject == gameObject)
            {
                ToggleCraftingUI();
            }
        }
    }

    private void ToggleCraftingUI()
    {
        isCraftingActive = !isCraftingActive;
        if (craftingUI != null)
        {
            craftingUI.SetActive(isCraftingActive);
            if (playerCam != null)
            {
                playerCam.LockMovement(isCraftingActive);
            }
            if (playerController != null)
            {
                playerController.enabled = !isCraftingActive;
            }
            if (crosshair != null)
            {
                crosshair.SetBool("IsCrafting", isCraftingActive);
            }

            Debug.Log($"Crafting UI {(isCraftingActive ? "opened" : "closed")}");
        }
    }
}
