using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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

    [Header("Brightness Multiplier")]
    [SerializeField] private float brightnessMultiplier = 0.5f;

    [Header("Distance Fade")]
    [SerializeField] private bool controlSceneLighting = true;
    [SerializeField] private bool controlDistanceFog = true;
    [SerializeField] private bool useDistanceFog = true;
    [SerializeField] private Color fogColor = new Color(0.01f, 0.01f, 0.018f);
    [SerializeField] private float fogStartDistance = 20f;
    [SerializeField] private float fogEndDistance = 55f;

    [Header("Background")]
    [SerializeField] private bool useSolidDarkBackground = true;
    [SerializeField] private Color backgroundColor = new Color(0.005f, 0.005f, 0.012f);
    [SerializeField] private bool removeSkybox = true;
    [SerializeField] private bool applyToAllCameras = true;
    [SerializeField] private Camera targetCamera;

    [Header("Debug")]
#pragma warning disable CS0414
    [SerializeField] private bool autoApplySceneRoomsOnStart = true;
#pragma warning restore CS0414
    [SerializeField] private bool liveUpdateInPlayMode = true;
    [SerializeField] private bool logAppliedValues;

    private float originalMainLightIntensity = -1f;
    private float cachedAverageRoomArea;
    private bool hasAppliedLighting;
    private NativeRoomLightingZone activeNativeRoomZone;

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
        float finalDarkness = CalculateEffectiveDarkness();

        hasAppliedLighting = true;
        ApplyDarkness(finalDarkness);

        if (logAppliedValues)
        {
            Debug.Log($"Dungeon lighting applied. Darkness: {finalDarkness:0.00}, average room area: {cachedAverageRoomArea:0.0}");
        }
    }

    public void ApplyLightingFromSceneRooms()
    {
        RoomInstance[] sceneRooms = FindObjectsByType<RoomInstance>(FindObjectsSortMode.None);
        List<GameObject> roomObjects = new List<GameObject>();

        for (int i = 0; i < sceneRooms.Length; i++)
        {
            if (sceneRooms[i] != null && sceneRooms[i].AreaShape == MapAreaShape.Room)
            {
                roomObjects.Add(sceneRooms[i].gameObject);
            }
        }

        ApplyLighting(roomObjects);
    }

    public void ApplyDarkness(float darkness)
    {
        CurrentDarkness = Mathf.Clamp01(darkness);

        if (controlSceneLighting)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            
            float brightness = Options.BrightnessOffset;
            Color adjustedBrightAmbient = brightAmbientColor + (Color.white * brightness * 0.3f);
            RenderSettings.ambientLight = Color.Lerp(adjustedBrightAmbient, darkAmbientColor, CurrentDarkness);
            
            float ambientIntensity = Mathf.Lerp(brightAmbientIntensity, darkAmbientIntensity, CurrentDarkness);
            RenderSettings.ambientIntensity = Mathf.Clamp(ambientIntensity, 0.05f, 2f);

            ResolveMainLight();
            CacheOriginalMainLightIntensity();

            if (mainLight != null)
            {
                float multiplier = Mathf.Lerp(brightMainLightMultiplier, darkMainLightMultiplier, CurrentDarkness);
                mainLight.intensity = originalMainLightIntensity * multiplier;
            }
        }

        ApplyBackground();
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

    public void SetNativeRoomZone(NativeRoomLightingZone zone)
    {
        if (zone == null)
        {
            return;
        }

        activeNativeRoomZone = zone;
        ApplyDarkness(CalculateEffectiveDarkness());
    }

    public void ClearNativeRoomZone(NativeRoomLightingZone zone)
    {
        if (activeNativeRoomZone != zone)
        {
            return;
        }

        activeNativeRoomZone = null;
        ApplyDarkness(CalculateEffectiveDarkness());
    }

    private float CalculateEffectiveDarkness()
    {
        float globalDarkness = CalculateDarknessFromCachedArea();

        if (!controlSceneLighting || activeNativeRoomZone == null || activeNativeRoomZone.UseDungeonManagerBaseLighting)
        {
            return globalDarkness;
        }

        if (activeNativeRoomZone.UseRoomSizeInfluence)
        {
            float area = 0f;
            if (TryGetPieceBounds(activeNativeRoomZone.gameObject, activeNativeRoomZone, out Bounds bounds))
            {
                area = Mathf.Abs(bounds.size.x * bounds.size.z);
            }
            float sizeFactor = Mathf.InverseLerp(minRoomArea, maxRoomArea, area);
            float finalDarkness = activeNativeRoomZone.Darkness + (sizeFactor * activeNativeRoomZone.RoomSizeDarknessInfluence);
            return Mathf.Clamp01(finalDarkness);
        }

        if (activeNativeRoomZone.OverrideDarkness)
        {
            return Mathf.Clamp01(activeNativeRoomZone.Darkness);
        }

        if (activeNativeRoomZone.AddToGlobalDarkness)
        {
            return Mathf.Clamp01(globalDarkness + activeNativeRoomZone.DarknessOffset);
        }

        return globalDarkness;
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
        if (!controlDistanceFog)
        {
            return;
        }

        RenderSettings.fog = useDistanceFog;
        if (!useDistanceFog)
        {
            return;
        }

        RenderSettings.fogMode = FogMode.Linear;

        if (activeNativeRoomZone != null)
        {
            if (activeNativeRoomZone.OverrideFog)
            {
                RenderSettings.fogColor = activeNativeRoomZone.FogColor;
                RenderSettings.fogStartDistance = activeNativeRoomZone.FogStartDistance;
                RenderSettings.fogEndDistance = activeNativeRoomZone.FogEndDistance;
                return;
            }

            if (activeNativeRoomZone.ModifyFog)
            {
                RenderSettings.fogColor = fogColor;
                float newStart = fogStartDistance + activeNativeRoomZone.FogStartDistanceOffset;
                float newEnd = fogEndDistance + activeNativeRoomZone.FogEndDistanceOffset;
                RenderSettings.fogStartDistance = Mathf.Max(0f, newStart);
                RenderSettings.fogEndDistance = Mathf.Max(RenderSettings.fogStartDistance + 0.01f, newEnd);
                return;
            }
        }

        RenderSettings.fogColor = fogColor;
        RenderSettings.fogStartDistance = fogStartDistance;
        RenderSettings.fogEndDistance = fogEndDistance;
    }

private void ApplyBackground()
    {
        if (!useSolidDarkBackground)
        {
            return;
        }

        if (removeSkybox)
        {
            RenderSettings.skybox = null;
        }

        if (applyToAllCameras)
        {
            Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            for (int i = 0; i < cameras.Length; i++)
            {
                ApplyBackgroundToCamera(cameras[i]);
            }

            return;
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        ApplyBackgroundToCamera(targetCamera);
    }

    private void ApplyBackgroundToCamera(Camera cameraToApply)
    {
        if (cameraToApply == null)
        {
            return;
        }

        cameraToApply.clearFlags = CameraClearFlags.SolidColor;
        cameraToApply.backgroundColor = backgroundColor;
    }

    public void UpdateBrightness()
    {
        if (hasAppliedLighting)
        {
            ApplyDarkness(CalculateEffectiveDarkness());
            return;
        }
        
        float brightness = Options.BrightnessOffset;
        float currentDarkness = baseDarkness;
        Color adjustedBrightAmbient = brightAmbientColor + (Color.white * brightness * 0.3f);
        RenderSettings.ambientLight = Color.Lerp(adjustedBrightAmbient, darkAmbientColor, currentDarkness);
        RenderSettings.ambientIntensity = Mathf.Lerp(brightAmbientIntensity, darkAmbientIntensity, currentDarkness);
        
        ResolveMainLight();
        CacheOriginalMainLightIntensity();
        if (mainLight != null)
        {
            float multiplier = Mathf.Lerp(brightMainLightMultiplier, darkMainLightMultiplier, currentDarkness);
            mainLight.intensity = originalMainLightIntensity * multiplier;
        }
    }

    public void SetBaseIntensities(float ambient, float mainLightIntensity)
    {
        darkAmbientIntensity = ambient;
        if (mainLight != null)
        {
            originalMainLightIntensity = mainLightIntensity;
        }
    }
}
