using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class NativeRoomLightingZone : RoomInstance
{
    [Header("Native Room")]
    [SerializeField] private bool discoverOnStart;
    [SerializeField] private MapAreaShape nativeAreaShape = MapAreaShape.Room;

    [Header("Collision")]
    [Tooltip("Pone esta zona en el layer Rooms para que los proyectiles existentes la ignoren.")]
    [SerializeField] private bool useRoomsLayerForProjectiles = true;
    [SerializeField] private string projectileSafeLayerName = "Rooms";

    [Header("Darkness Override")]
    [Tooltip("Si esta activo, la zona usa la iluminacion base del DungeonLightingManager y no modifica la oscuridad.")]
    [SerializeField] private bool useDungeonManagerBaseLighting = true;

    [SerializeField] private bool overrideDarkness = true;
    [Tooltip("El valor de oscuridad a usar. Si 'Use Room Size Influence' está activo, este es el valor base.")]
    [Range(0f, 1f)]
    [SerializeField] private float darkness = 0.45f;

    [Tooltip("Si está activo, la oscuridad se calculará usando el tamaño de la zona, similar al generador de mazmorras.")]
    [SerializeField] private bool useRoomSizeInfluence = false;
    [Tooltip("Cantidad de oscuridad adicional para las zonas más grandes. Solo si 'Use Room Size Influence' está activo.")]
    [Range(0f, 1f)]
    [SerializeField] private float roomSizeDarknessInfluence = 0.2f;

    [Space]
    [SerializeField] private bool addToGlobalDarkness;
    [Range(-1f, 1f)]
    [SerializeField] private float darknessOffset;

    [Header("Distance Fade Override")]
    [SerializeField] private bool overrideFog;
    [SerializeField] private Color fogColor = new Color(0.01f, 0.01f, 0.018f);
    [SerializeField] private float fogStartDistance = 18f;
    [SerializeField] private float fogEndDistance = 45f;

    [Space]
    [Tooltip("Modifica los valores de niebla globales en lugar de sobreescribirlos. 'Override Fog' debe estar desactivado.")]
    [SerializeField] private bool modifyFog;
    [SerializeField] private float fogStartDistanceOffset = 10f;
    [SerializeField] private float fogEndDistanceOffset = 15f;

    public bool OverrideDarkness => overrideDarkness;
    public bool UseDungeonManagerBaseLighting => useDungeonManagerBaseLighting;
    public float Darkness => darkness;
    public bool UseRoomSizeInfluence => useRoomSizeInfluence;
    public float RoomSizeDarknessInfluence => roomSizeDarknessInfluence;
    public bool AddToGlobalDarkness => addToGlobalDarkness;
    public float DarknessOffset => darknessOffset;
    public bool OverrideFog => overrideFog;
    public Color FogColor => fogColor;
    public float FogStartDistance => fogStartDistance;
    public float FogEndDistance => fogEndDistance;
    public bool ModifyFog => modifyFog;
    public float FogStartDistanceOffset => fogStartDistanceOffset;
    public float FogEndDistanceOffset => fogEndDistanceOffset;

    private DungeonLightingManager lightingManager;
    private bool playerInside;

    private void Reset()
    {
        Collider zoneCollider = GetComponent<Collider>();
        zoneCollider.isTrigger = true;
        ApplyProjectileSafeLayer();
    }

    private void Awake()
    {
        Collider zoneCollider = GetComponent<Collider>();
        if (zoneCollider != null)
        {
            zoneCollider.isTrigger = true;
        }

        ApplyProjectileSafeLayer();
    }

    private void OnValidate()
    {
        Collider zoneCollider = GetComponent<Collider>();
        if (zoneCollider != null)
        {
            zoneCollider.isTrigger = true;
        }

        fogEndDistance = Mathf.Max(fogStartDistance + 0.01f, fogEndDistance);
    }

    private void Start()
    {
        ApplyProjectileSafeLayer();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInside = true;
        ResolveLightingManager();

        if (lightingManager != null)
        {
            lightingManager.SetNativeRoomZone(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInside = false;

        if (lightingManager != null)
        {
            lightingManager.ClearNativeRoomZone(this);
        }
    }

    private void OnDisable()
    {
        if (playerInside && lightingManager != null)
        {
            lightingManager.ClearNativeRoomZone(this);
        }
    }

    private void ResolveLightingManager()
    {
        if (lightingManager == null)
        {
            lightingManager = FindFirstObjectByType<DungeonLightingManager>();
        }
    }

    private void ApplyProjectileSafeLayer()
    {
        if (!useRoomsLayerForProjectiles || string.IsNullOrWhiteSpace(projectileSafeLayerName))
        {
            return;
        }

        int safeLayer = LayerMask.NameToLayer(projectileSafeLayerName);
        if (safeLayer >= 0)
        {
            gameObject.layer = safeLayer;
        }
    }
}
