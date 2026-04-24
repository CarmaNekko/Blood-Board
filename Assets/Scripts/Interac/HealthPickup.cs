using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Ajustes de Curación")]
    [SerializeField] private float healAmount = 25f;

    [Header("Movimiento (Flotar y Rotar)")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float bobIntensity = 0.5f;
    [SerializeField] private float bobSpeed = 2f;

    [Header("Efecto de Parpadeo (Emisión)")]
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 2.5f;
    [SerializeField] private float pulseSpeed = 5f;

    private Vector3 startPosition;
    private Material itemMaterial;
    private Color baseColor;

    void Start()
    {
        startPosition = transform.position;

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            itemMaterial = renderer.material;
            baseColor = itemMaterial.GetColor("_EmissionColor");
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobIntensity;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        if (itemMaterial != null)
        {
            float pulse = Mathf.Lerp(minIntensity, maxIntensity, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            itemMaterial.SetColor("_EmissionColor", baseColor * pulse);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.RestoreHealth(healAmount);
                Destroy(gameObject);
            }
        }
    }
}