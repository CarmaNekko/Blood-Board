using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class DungeonLightingManager : MonoBehaviour
{
    [Header("Global Darkness")]
    [Range(0f, 1f)]
    [SerializeField] private float baseDarkness = 0.65f;
    [Range(0f, 1f)]
    [SerializeField] private float roomSizeDarknessInfluence = 0.2f;
    [SerializeField] private float minRoomArea = 250f;
    [SerializeField] private float maxRoomArea = 1600f;

    [Header("Ambient Light")]
    [SerializeField] private Color brightAmbientColor = new Color(0.2f, 0.22f, 0.25f);
    [SerializeField] private Color darkAmbientColor = new Color(0.025f, 0.025f, 0.035f);
    [SerializeField] private float brightAmbientIntensity = 0.85f;
    [SerializeField] private float darkAmbientIntensity = 0.18f;

    [Header("Main Light")]
    [SerializeField] private Light mainLight;
    [Range(0f, 1f)]
    [SerializeField] private float brightMainLightMultiplier = 0.65f;
    [Range(0f, 1f)]
    [SerializeField] private float darkMainLightMultiplier = 0.18f;

    [Header("Distance Fade")]
    [SerializeField] private bool useDistanceFog = true;
    [SerializeField] private Color fogColor = new Color(0.01f, 0.01f, 0.018f);
    [SerializeField] private float fogStartDistance = 20f;
    [SerializeField] private float fogEndDistance = 55f;

    [Header("Debug")]
    [SerializeField] private bool liveUpdateInPlayMode = true;
    [SerializeField] private bool logAppliedValues;

    private float originalMainLightIntensity = -1f;
    private float cachedAverageRoomArea;
    private bool hasAppliedLighting;

    public float CurrentDarkness { get; private set; }

    private void Awake()
    {
        ResolveMainLight();
        CacheOriginalMainLightIntensity();
    }

    private void Update()
    {
        if (!liveUpdateInPlayMode || !hasAppliedLighting)
        {
            return;
        }

        ApplyDarkness(CalculateDarknessFromCachedArea());
    }

    private void OnValidate()
    {
        minRoomArea = Mathf.Max(0.01f, minRoomArea);
        maxRoomArea = Mathf.Max(minRoomArea + 0.01f, maxRoomArea);
        fogEndDistance = Mathf.Max(fogStartDistance + 0.01f, fogEndDistance);

        if (!Application.isPlaying)
        {
            return;
        }

        ResolveMainLight();
        CacheOriginalMainLightIntensity();

        if (hasAppliedLighting)
        {
            ApplyDarkness(CalculateDarknessFromCachedArea());
        }
    }

    public void ApplyLighting(IReadOnlyList<GameObject> generatedPieces)
    {
        cachedAverageRoomArea = CalculateAverageRoomArea(generatedPieces);
        float finalDarkness = CalculateDarknessFromCachedArea();

        hasAppliedLighting = true;
        ApplyDarkness(finalDarkness);

        if (logAppliedValues)
        {
            Debug.Log($"Dungeon lighting applied. Darkness: {finalDarkness:0.00}, average room area: {cachedAverageRoomArea:0.0}");
        }
    }

    public void ApplyDarkness(float darkness)
    {
        CurrentDarkness = Mathf.Clamp01(darkness);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.Lerp(brightAmbientColor, darkAmbientColor, CurrentDarkness);
        RenderSettings.ambientIntensity = Mathf.Lerp(brightAmbientIntensity, darkAmbientIntensity, CurrentDarkness);

        ResolveMainLight();
        CacheOriginalMainLightIntensity();

        if (mainLight != null)
        {
            float multiplier = Mathf.Lerp(brightMainLightMultiplier, darkMainLightMultiplier, CurrentDarkness);
            mainLight.intensity = originalMainLightIntensity * multiplier;
        }

        ApplyDistanceFog();
    }

    private float CalculateDarknessFromCachedArea()
    {
        float averageRoomArea = cachedAverageRoomArea > 0f ? cachedAverageRoomArea : minRoomArea;
        float sizeFactor = Mathf.InverseLerp(minRoomArea, maxRoomArea, averageRoomArea);
        return Mathf.Clamp01(baseDarkness + (sizeFactor * roomSizeDarknessInfluence));
    }

    private float CalculateAverageRoomArea(IReadOnlyList<GameObject> generatedPieces)
    {
        if (generatedPieces == null || generatedPieces.Count == 0)
        {
            return minRoomArea;
        }

        float totalArea = 0f;
        int roomCount = 0;

        for (int i = 0; i < generatedPieces.Count; i++)
        {
            GameObject piece = generatedPieces[i];
            if (piece == null)
            {
                continue;
            }

            RoomInstance roomInstance = piece.GetComponent<RoomInstance>();
            if (roomInstance != null && roomInstance.AreaShape != MapAreaShape.Room)
            {
                continue;
            }

            if (!TryGetPieceBounds(piece, roomInstance, out Bounds bounds))
            {
                continue;
            }

            float area = Mathf.Abs(bounds.size.x * bounds.size.z);
            if (area <= 0f)
            {
                continue;
            }

            totalArea += area;
            roomCount++;
        }

        return roomCount > 0 ? totalArea / roomCount : minRoomArea;
    }

    private bool TryGetPieceBounds(GameObject piece, RoomInstance roomInstance, out Bounds bounds)
    {
        if (roomInstance != null && roomInstance.HasBounds)
        {
            bounds = roomInstance.WorldBounds;
            return true;
        }

        BoxCollider boxCollider = piece.GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            bounds = boxCollider.bounds;
            return true;
        }

        Collider[] colliders = piece.GetComponentsInChildren<Collider>();
        bool hasBounds = false;
        bounds = default;

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider pieceCollider = colliders[i];
            if (!pieceCollider.enabled)
            {
                continue;
            }

            if (!hasBounds)
            {
                bounds = pieceCollider.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(pieceCollider.bounds);
            }
        }

        return hasBounds;
    }

    private void ResolveMainLight()
    {
        if (mainLight != null)
        {
            return;
        }

        Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null && lights[i].type == LightType.Directional)
            {
                mainLight = lights[i];
                return;
            }
        }
    }

    private void CacheOriginalMainLightIntensity()
    {
        if (originalMainLightIntensity >= 0f || mainLight == null)
        {
            return;
        }

        originalMainLightIntensity = mainLight.intensity;
    }

    private void ApplyDistanceFog()
    {
        RenderSettings.fog = useDistanceFog;

        if (!useDistanceFog)
        {
            return;
        }

        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogStartDistance = fogStartDistance;
        RenderSettings.fogEndDistance = fogEndDistance;
    }
}
