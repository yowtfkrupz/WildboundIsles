using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Health")]
    public float maxHealth;
    public float currentHealth;
    public float healthregenSpeed = 3f;
    private float healthRegenTimer;
    public Image healthBar;
    public TextMeshProUGUI HPammount;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaDrainSpeed = 10f;
    public float staminaRegenSpeed = 5f;
    public float staminaRegenDelay = 3f;
    private float staminaRegenTimer;
    public Image staminaBar;
    public TextMeshProUGUI staminaAmmount;

    [Header("UI and Camera")]
    public GameObject UIContainer;
    public GameObject Deathscreen;
    public GameObject cameraHolder;
    public PostProcessProfile deathProfile;


    [Header("Infinite Stamina")]
    public bool hasInfiniteStamina = false;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;

        currentStamina = maxStamina;
        currentHealth = maxHealth;
        HPammount.text = currentHealth.ToString();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        healthRegenTimer = 0f;
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private bool isDead = false;

    private void Die()
    {
        Debug.Log("Hráè zemøel!");

        isDead = true;
        Vector3 deathPosition = transform.position;

        Vector3 lastPlayerPosition = transform.position;

        if (cameraHolder != null)
        {
            cameraHolder.transform.parent = null;

            if (UIContainer != null)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Deathscreen.SetActive(true);
                UIContainer.SetActive(false);
            }
            else
            {
                Debug.LogWarning("UIContainer nebyl pøiøazen v Inspectoru!");
            }

            PostProcessVolume volume = cameraHolder.GetComponent<PostProcessVolume>();
            if (volume != null && deathProfile != null)
            {
                volume.profile = deathProfile;
            }

            StartCoroutine(LevitatingCamera(cameraHolder.transform, deathPosition, lastPlayerPosition));
        }
        else
        {
            Debug.LogWarning("CameraHolder nebyl nalezen!");
        }
    }

    private IEnumerator LevitatingCamera(Transform cameraTransform, Vector3 center, Vector3 lastPlayerPosition)
    {
        float rotationSpeed = 30f;
        float radius = 5f;
        float elapsed = 0f;

        while (true)
        {
            elapsed += Time.deltaTime;

            float angle = elapsed * rotationSpeed;
            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            float z = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;

            cameraTransform.position = center + new Vector3(x, 2f, z);

            Quaternion targetRotation = Quaternion.LookRotation(lastPlayerPosition - cameraTransform.position);
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetRotation, Time.deltaTime * 2f);

            yield return null;
        }
    }


    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            HPammount.text = currentHealth.ToString();
            healthBar.fillAmount = currentHealth / maxHealth;
        }
    }

    private void Update()
    {
        if (isDead)
            return;

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;

        if (hasInfiniteStamina)
        {
            staminaBar.fillAmount = 1f;
            currentStamina = maxStamina;
        }
        else
        {
            staminaAmmount.text = Mathf.Round(currentStamina).ToString();
            staminaBar.fillAmount = currentStamina / maxStamina;

            if (state != MovementState.sprinting && currentStamina < maxStamina)
            {
                if (staminaRegenTimer >= staminaRegenDelay)
                {
                    currentStamina += staminaRegenSpeed * Time.deltaTime;
                    currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
                }
                else
                {
                    staminaRegenTimer += Time.deltaTime;
                }
            }

            if (state == MovementState.sprinting && currentStamina > 0)
            {
                currentStamina -= staminaDrainSpeed * Time.deltaTime;
                currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

                staminaRegenTimer = 0f;
            }
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        if (isDead)
            return;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler()
    {
        if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        else if (grounded && Input.GetKey(sprintKey) && currentStamina > 0)
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        else if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private bool isRooted = false;

    public void ApplyRoot(float duration)
    {
        if (isRooted)
            return;

        Debug.Log($"Player is rooted for {duration} seconds.");
        isRooted = true;

        rb.velocity = Vector3.zero;

        StartCoroutine(RootEffect(duration));
    }

    private IEnumerator RootEffect(float duration)
    {
        yield return new WaitForSeconds(duration);

        Debug.Log("Root effect ended.");
        isRooted = false;
    }

    private void Jump()
    {
        exitingSlope = true;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}