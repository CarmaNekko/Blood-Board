using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Attributes")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float jumpForce = 2f;
    [SerializeField] private float gravity = -19.81f;

    [Header("Camera Settings")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float mouseSensitivity = 200f;
    public static float GlobalMouseSensitivity { get; private set; }

    [Header("Ground Radar")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    private bool isActuallyGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        GlobalMouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", mouseSensitivity);
    }

    void Update()
    {
        if (PauseScreen.IsPaused || TutorialMessage.IsTutorialActive)
        {
            return;
        }

        isActuallyGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        float mouseX = Input.GetAxis("Mouse X") * GlobalMouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * GlobalMouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        if (isActuallyGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (Input.GetButtonDown("Jump") && isActuallyGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public static void SetGlobalMouseSensitivity(float sensitivity)
    {
        GlobalMouseSensitivity = Mathf.Clamp(sensitivity, 10f, 1000f);
    }
}
