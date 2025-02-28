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
    public float staminaRegenDelay = 3f;  // Delay before stamina regenerates
    private float staminaRegenTimer; // Timer to track time since sprinting stopped
    public Image staminaBar;
    public TextMeshProUGUI staminaAmmount;

    [Header("UI and Camera")]
    public GameObject UIContainer; // Prázdný objekt obsahující UI
    public GameObject Deathscreen;
    public GameObject cameraHolder; // CameraHolder obsahující kameru
    public PostProcessProfile deathProfile; // Post-process profil pro efekt smrti


    [Header("Infinite Stamina")]
    public bool hasInfiniteStamina = false; // Zda má hráè nekoneènou staminu

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

    private bool isDead = false; // Pøidána promìnná pro sledování smrti hráèe

    private void Die()
    {
        Debug.Log("Hráè zemøel!");

        isDead = true; // Nastav hráèe jako mrtvého

        // Získej pozici smrti
        Vector3 deathPosition = transform.position;

        Vector3 lastPlayerPosition = transform.position;

        if (cameraHolder != null)
        {
            // Odpoj CameraHolder od hráèe
            cameraHolder.transform.parent = null;

            // Skryj UI Container
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

            // Nastav nový post-process profil
            PostProcessVolume volume = cameraHolder.GetComponent<PostProcessVolume>();
            if (volume != null && deathProfile != null)
            {
                volume.profile = deathProfile;
            }

            // Aktivuj rotaci kamery kolem pozice smrti
            StartCoroutine(LevitatingCamera(cameraHolder.transform, deathPosition, lastPlayerPosition));
        }
        else
        {
            Debug.LogWarning("CameraHolder nebyl nalezen!");
        }
    }

    private IEnumerator LevitatingCamera(Transform cameraTransform, Vector3 center, Vector3 lastPlayerPosition)
    {
        float rotationSpeed = 30f; // Rychlost rotace
        float radius = 5f; // Polomìr kruhu kolem pozice smrti
        float elapsed = 0f;

        while (true) // Nekoneèný loop
        {
            elapsed += Time.deltaTime;

            // Vypoèti novou pozici kamery na kruhu
            float angle = elapsed * rotationSpeed;
            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            float z = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;

            cameraTransform.position = center + new Vector3(x, 2f, z); // Pøidáme výšku

            // Kamera se postupnì otáèí smìrem k poslední pozici hráèe
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
            return; // Pokud je hráè mrtvý, ignoruj vstupy

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
            // Zaokrouhlení staminy pro zobrazení
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
            return; // Pokud je hráè mrtvý, ignoruj vstupy

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

    private bool isRooted = false; // Indikátor, zda je hráè znehybnìn

    public void ApplyRoot(float duration)
    {
        if (isRooted)
            return;

        Debug.Log($"Player is rooted for {duration} seconds.");
        isRooted = true;

        // Zastav pohyb
        rb.velocity = Vector3.zero;

        // Zakázat vstup
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