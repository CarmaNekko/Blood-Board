using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Attributes")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float sprintSpeed = 18f;
    [SerializeField] private float jumpForce = 2f;
    [SerializeField] private float gravity = -19.81f;

    [Header("Camera Settings")]
    [SerializeField] private Transform playerCamera;
    private const float MouseLookMultiplier = 0.05f;
    public static float GlobalMouseSensitivity { get; private set; }

    [Header("Ground Radar")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [Header("Físicas de Impacto")]
    [SerializeField] private float mass = 3f;
    private Vector3 impactVelocity = Vector3.zero;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    private bool isActuallyGrounded;

    public bool IsSprinting { get; private set; }
    public float CurrentInputX { get; private set; }
    public bool IsGrounded => isActuallyGrounded;
    public Vector3 CurrentVelocity => controller.velocity;
    public float CameraTilt { get; set; }
    private bool wasSprintingWhenJumped = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 200f);
        SetGlobalMouseSensitivity(savedSensitivity);
    }

    void Update()
    {
        if (PauseScreen.IsPaused || TutorialMessage.IsTutorialActive)
        {
            return;
        }

        float mouseX = Input.GetAxisRaw("Mouse X") * GlobalMouseSensitivity * MouseLookMultiplier;
        float mouseY = Input.GetAxisRaw("Mouse Y") * GlobalMouseSensitivity * MouseLookMultiplier;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, CameraTilt);
        transform.Rotate(Vector3.up * mouseX);

        isActuallyGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        CurrentInputX = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if (isActuallyGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            wasSprintingWhenJumped = false;
        }

        if (Input.GetButtonDown("Jump") && isActuallyGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

            if (Input.GetKey(KeyCode.LeftShift) && z > 0)
            {
                wasSprintingWhenJumped = true;
            }
        }

        bool isSprintingOnGround = Input.GetKey(KeyCode.LeftShift) && z > 0 && isActuallyGrounded;
        IsSprinting = isSprintingOnGround || (!isActuallyGrounded && wasSprintingWhenJumped);

        float currentSpeed = IsSprinting ? sprintSpeed : moveSpeed;

        Vector3 horizontalVelocity = (transform.right * CurrentInputX + transform.forward * z) * currentSpeed;

        Vector3 currentKnockback = Vector3.zero;
        if (impactVelocity.magnitude > 0.2f)
        {
            currentKnockback = impactVelocity;
            impactVelocity = Vector3.Lerp(impactVelocity, Vector3.zero, 5f * Time.deltaTime);
        }
        else
        {
            impactVelocity = Vector3.zero;
        }

        velocity.y += gravity * Time.deltaTime;

        Vector3 finalMovement = horizontalVelocity + currentKnockback + velocity;

        controller.Move(finalMovement * Time.deltaTime);
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        Debug.Log("se aplicó el knockback");
        direction.Normalize();
        if (direction.y < 0) direction.y = -direction.y;
        direction.y += 0.5f;
        impactVelocity += direction * force / mass;
    }

    public static void SetGlobalMouseSensitivity(float sensitivity)
    {
        GlobalMouseSensitivity = Mathf.Clamp(sensitivity, 0f, 400f);
    }
}