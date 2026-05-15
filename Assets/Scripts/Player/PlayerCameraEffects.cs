using UnityEngine;

public class PlayerCameraEffects : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerMovement playerMovement;
    private Camera cam;

    [Header("1. FOV Dinámico (Efecto Velocidad)")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float sprintFOV = 75f;
    [SerializeField] private float fovTransitionSpeed = 8f;

    [Header("2. Inclinación (Strafe Tilt)")]
    [SerializeField] private float maxTiltAngle = 3.5f;
    [SerializeField] private float tiltSpeed = 6f;
    private float currentTilt;

    [Header("3. Cabeceo (Head Bobbing)")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.1f;
    private float defaultYPos;
    private float bobTimer;

    [Header("4. Impacto de Caída (Landing Dip)")]
    [SerializeField] private float dipAmount = 0.4f;
    [SerializeField] private float dipRecoverySpeed = 10f;
    private float currentDip;
    private bool wasGrounded;

    [Header("5. Retroceso de Disparo (Recoil)")]
    [SerializeField] private float recoilKickAmount = 0.4f; // Cuánto empuja hacia atrás
    [SerializeField] private float recoilRecoverySpeed = 15f; // Qué tan rápido regresa
    private float currentRecoilZ;

    void Start()
    {
        cam = GetComponent<Camera>();
        defaultYPos = transform.localPosition.y;
        wasGrounded = playerMovement.IsGrounded;

        // Si no asignaste el PlayerMovement en el inspector, lo busca automáticamente
        if (playerMovement == null)
            playerMovement = GetComponentInParent<PlayerMovement>();
    }

    void Update()
    {
        if (PauseScreen.IsPaused || TutorialMessage.IsTutorialActive) return;

        HandleDynamicFOV();
        HandleStrafeTilt();
        HandleHeadBobbing();
        HandleLandingDip();
    }

    private void HandleDynamicFOV()
    {
        float targetFOV = playerMovement.IsSprinting ? sprintFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
    }

    private void HandleStrafeTilt()
    {
        // Multiplicamos por negativo para que se incline HACIA el lado al que vamos
        float targetTilt = -playerMovement.CurrentInputX * maxTiltAngle;
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        // Le pasamos esta inclinación al script de movimiento para que la sume a la rotación del ratón
        playerMovement.CameraTilt = currentTilt;
    }

    private void HandleHeadBobbing()
    {
        // Solo hacemos bobbing si nos estamos moviendo sobre el suelo
        if (playerMovement.IsGrounded && playerMovement.CurrentVelocity.magnitude > 0.1f)
        {
            float speed = playerMovement.IsSprinting ? sprintBobSpeed : walkBobSpeed;
            float amount = playerMovement.IsSprinting ? sprintBobAmount : walkBobAmount;

            bobTimer += Time.deltaTime * speed;
            float newY = defaultYPos + Mathf.Sin(bobTimer) * amount;

            // Aplicamos la posición Y del cabeceo (sumándole el dip de la caída por si acaso)
            transform.localPosition = new Vector3(transform.localPosition.x, newY + currentDip, currentRecoilZ);
        }
        else
        {
            // Volver suavemente al centro si nos detenemos
            bobTimer = 0;
            float newY = Mathf.Lerp(transform.localPosition.y, defaultYPos + currentDip, Time.deltaTime * 5f);
            transform.localPosition = new Vector3(transform.localPosition.x, newY, currentRecoilZ);
        }
        currentRecoilZ = Mathf.Lerp(currentRecoilZ, 0f, Time.deltaTime * recoilRecoverySpeed);
    }

    private void HandleLandingDip()
    {
        // Detectar el momento exacto en que tocamos el suelo
        if (playerMovement.IsGrounded && !wasGrounded)
        {
            // Si veníamos cayendo rápido, aplicamos el impacto hacia abajo
            currentDip = -dipAmount;
        }

        // Recuperarse suavemente del impacto (volver a 0)
        currentDip = Mathf.Lerp(currentDip, 0f, Time.deltaTime * dipRecoverySpeed);
        wasGrounded = playerMovement.IsGrounded;
    }
    public void ApplyShootRecoil()
    {
        currentRecoilZ = -recoilKickAmount;
    }
}
