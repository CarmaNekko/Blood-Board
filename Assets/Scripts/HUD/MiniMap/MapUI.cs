using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MapUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform contentRoot;

    [Header("Frame")]
    [SerializeField] private bool autoConfigureContainer = true;
    [SerializeField] private Vector2 panelSize = new Vector2(300f, 300f);
    [SerializeField] private Vector2 panelMargin = new Vector2(24f, 24f);
    [SerializeField] private float mapPadding = 22f;

    [Header("Temporary Expand")]
    [SerializeField] private bool allowTemporaryExpand = true;
    [SerializeField] private KeyCode expandHoldKey = KeyCode.Tab;
    [SerializeField] private float expandedScale = 1.75f;
    [SerializeField] private float expandLerpSpeed = 10f;

    [Header("Adaptive Scale")]
    [SerializeField] private float collapsedPixelsPerWorldUnit = 7f;
    [SerializeField] private float expandedMinPixelsPerWorldUnit = 0.5f;
    [SerializeField] private float minPixelsPerWorldUnit = 3f;
    [SerializeField] private float maxPixelsPerWorldUnit = 30f;
    [SerializeField] private float redrawMoveThreshold = 0.75f;
    [SerializeField] private float minimumCorridorThickness = 10f;
    [SerializeField] private float minimumRoomSide = 18f;

    [Header("Fallback Exploration")]
    [SerializeField] private bool enableExplorationFallback = true;
    [SerializeField] private float explorationCellWorldSize = 6f;
    [SerializeField] private int maxExplorationCells = 300;

    [Header("Player Marker")]
    [SerializeField] private Vector2 currentMarkerSize = new Vector2(18f, 18f);
    [SerializeField] private Color currentMarkerColor = new Color(0.45f, 1f, 0.35f, 1f);
    [SerializeField] private Color currentRoomColor = new Color(1f, 0.82f, 0.28f, 1f);
    [SerializeField] private Color currentCorridorColor = new Color(1f, 0.9f, 0.45f, 1f);

    [Header("Colors")]
    [SerializeField] private Color startRoomColor = new Color(0.35f, 0.75f, 1f, 1f);
    [SerializeField] private Color normalRoomColor = new Color(1f, 1f, 1f, 0.9f);
    [SerializeField] private Color finalRoomColor = new Color(1f, 0.55f, 0.35f, 1f);
    [SerializeField] private Color corridorColor = new Color(0.85f, 0.85f, 0.85f, 0.9f);

    private static Sprite cachedWhiteSprite;

    private readonly HashSet<Vector2Int> exploredCells = new HashSet<Vector2Int>();
    private readonly Queue<Vector2Int> exploredCellOrder = new Queue<Vector2Int>();

    private DungeonLayout activeLayout;
    private RectTransform containerRoot;
    private RectTransform roomsRoot;
    private Transform playerTransform;
    private Vector3 lastRedrawPlayerPosition;
    private bool hasLastRedrawPlayerPosition;
    private Vector2Int currentExplorationCell;
    private bool hasCurrentExplorationCell;
    private Vector2Int lastExplorationCell;
    private bool hasLastExplorationCell;
    private int explorationQuarterTurnsToUp;
    private bool hasExplorationOrientation;
    private float currentExpandScale = 1f;

    private void Reset()
    {
        contentRoot = transform as RectTransform;
    }

    private void Awake()
    {
        if (contentRoot == null)
        {
            contentRoot = transform as RectTransform;
        }

        EnsureUiRoots();
    }

    private void OnEnable()
    {
        LevelManager.LayoutGenerated += HandleLayoutGenerated;
        RoomInstance.AreasChanged += HandleAreasChanged;
    }

    private void Start()
    {
        if (LevelManager.CurrentLayout != null)
        {
            BindLayout(LevelManager.CurrentLayout);
        }
        else
        {
            Redraw();
        }
    }

    private void Update()
    {
        ResolvePlayer();
        UpdateTemporaryExpand();

        if (ShouldUseAreaRendering())
        {
            UpdateAreaRendering();
        }
        else if (enableExplorationFallback)
        {
            UpdateExplorationFallback();
        }
    }

    private void OnDisable()
    {
        LevelManager.LayoutGenerated -= HandleLayoutGenerated;
        RoomInstance.AreasChanged -= HandleAreasChanged;
        UnbindLayout();
    }

    private void HandleLayoutGenerated(DungeonLayout layout)
    {
        BindLayout(layout);
    }

    private void HandleAreasChanged()
    {
        Redraw();
    }

    private void BindLayout(DungeonLayout layout)
    {
        if (activeLayout == layout)
        {
            Redraw();
            return;
        }

        UnbindLayout();
        activeLayout = layout;

        if (activeLayout != null)
        {
            activeLayout.LayoutChanged += Redraw;

            // In tutorial scene, discover all rooms so the minimap is fully drawn from the start
            bool isTutorial = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level_Tuto";
            if (isTutorial)
            {
                for (int i = 0; i < activeLayout.Rooms.Count; i++)
                {
                    activeLayout.DiscoverRoom(i);
                }
            }
        }

        Redraw();
    }

    private void UnbindLayout()
    {
        if (activeLayout != null)
        {
            activeLayout.LayoutChanged -= Redraw;
        }

        activeLayout = null;
    }

    private void UpdateTemporaryExpand()
    {
        if (!allowTemporaryExpand)
        {
            currentExpandScale = 1f;
            ApplyContainerSize();
            return;
        }

        float oldScale = currentExpandScale;
        float targetScale = Input.GetKey(expandHoldKey) ? expandedScale : 1f;
        float deltaTime = Time.unscaledDeltaTime > 0f ? Time.unscaledDeltaTime : Time.deltaTime;
        currentExpandScale = Mathf.MoveTowards(currentExpandScale, targetScale, expandLerpSpeed * deltaTime);
        ApplyContainerSize();

        if (!Mathf.Approximately(oldScale, currentExpandScale))
        {
            Redraw();
        }
    }

    private void UpdateAreaRendering()
    {
        if (playerTransform == null)
        {
            return;
        }

        if (!hasLastRedrawPlayerPosition ||
            (playerTransform.position - lastRedrawPlayerPosition).sqrMagnitude >= redrawMoveThreshold * redrawMoveThreshold)
        {
            Redraw();
        }
    }

    private void UpdateExplorationFallback()
    {
        if (playerTransform == null)
        {
            return;
        }

        Vector2Int playerCell = WorldToExplorationCell(playerTransform.position);
        if (hasCurrentExplorationCell && playerCell == currentExplorationCell)
        {
            return;
        }

        if (hasLastExplorationCell && !hasExplorationOrientation && playerCell != lastExplorationCell)
        {
            explorationQuarterTurnsToUp = GetQuarterTurnsToUp(playerCell - lastExplorationCell);
            hasExplorationOrientation = true;
        }

        currentExplorationCell = playerCell;
        hasCurrentExplorationCell = true;
        lastExplorationCell = playerCell;
        hasLastExplorationCell = true;

        if (exploredCells.Add(playerCell))
        {
            exploredCellOrder.Enqueue(playerCell);
            while (exploredCellOrder.Count > maxExplorationCells)
            {
                exploredCells.Remove(exploredCellOrder.Dequeue());
            }
        }

        Redraw();
    }

    private void ResolvePlayer()
    {
        if (playerTransform != null)
        {
            return;
        }

        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null)
        {
            playerTransform = taggedPlayer.transform;
            return;
        }

        PlayerMovement playerMovement = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerTransform = playerMovement.transform;
        }
    }

    private bool ShouldUseAreaRendering()
    {
        bool isTutorial = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level_Tuto";
        IReadOnlyList<RoomInstance> areas = RoomInstance.ActiveInstances;
        for (int i = 0; i < areas.Count; i++)
        {
            if (areas[i] != null && (isTutorial || areas[i].IsDiscovered) && areas[i].HasBounds)
            {
                return true;
            }
        }

        return false;
    }

    private void Redraw()
    {
        ResolvePlayer();

        EnsureUiRoots();
        ClearChildren(roomsRoot);

        if (playerTransform != null)
        {
            lastRedrawPlayerPosition = playerTransform.position;
            hasLastRedrawPlayerPosition = true;
        }

        if (DrawAreaMap())
        {
            return;
        }

        if (enableExplorationFallback)
        {
            DrawExplorationFallback();
        }
    }

    private bool DrawAreaMap()
    {
        if (playerTransform == null)
        {
            return false;
        }

        List<RoomInstance> discoveredAreas = CollectDiscoveredAreas();
        if (discoveredAreas.Count == 0)
        {
            return false;
        }

        Vector3 playerPosition = playerTransform.position;
        bool isTutorial = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level_Tuto";

        if (isTutorial)
        {
            // 1. Calcular los límites totales (Bounds) de todo el mapa
            Bounds mapBounds = new Bounds();
            bool boundsInitialized = false;

            for (int i = 0; i < discoveredAreas.Count; i++)
            {
                IReadOnlyList<Bounds> segments = discoveredAreas[i].VisualSegments;
                for (int j = 0; j < segments.Count; j++)
                {
                    if (!boundsInitialized)
                    {
                        mapBounds = segments[j];
                        boundsInitialized = true;
                    }
                    else
                    {
                        mapBounds.Encapsulate(segments[j]);
                    }
                }
            }

            if (!boundsInitialized) return false;

            // 2. Calcular la escala EXACTA para que el mapa encaje en el panel saltando límites
            Vector2 availableSize = GetCurrentPanelSize() - new Vector2(mapPadding * 2f, mapPadding * 2f);
            float scaleX = availableSize.x / Mathf.Max(0.1f, mapBounds.size.x);
            float scaleZ = availableSize.y / Mathf.Max(0.1f, mapBounds.size.z);
            
            // Elegir la escala menor para asegurar que todo encaje sin recortarse
            float pixelsPerWorldUnit = Mathf.Min(scaleX, scaleZ);

            // 3. Dibujar las áreas relativas al CENTRO del mapa
            for (int i = 0; i < discoveredAreas.Count; i++)
            {
                RoomInstance area = discoveredAreas[i];
                IReadOnlyList<Bounds> segments = area.VisualSegments;

                for (int segmentIndex = 0; segmentIndex < segments.Count; segmentIndex++)
                {
                    Bounds segment = segments[segmentIndex];
                    Vector2 localCenter = new Vector2(
                        (segment.center.x - mapBounds.center.x) * pixelsPerWorldUnit,
                        (segment.center.z - mapBounds.center.z) * pixelsPerWorldUnit
                    );

                    // Pasamos "true" para evadir los límites que deformaban los pasillos a cuadrados enormes
                    Vector2 visualSize = GetAreaVisualSize(area, segment, pixelsPerWorldUnit, true);
                    CreateMapBlock(localCenter, visualSize, GetAreaColor(area));
                }
            }

            // 4. Dibujar al jugador relativo al centro del mapa
            Vector2 playerPosLocal = new Vector2(
                (playerPosition.x - mapBounds.center.x) * pixelsPerWorldUnit,
                (playerPosition.z - mapBounds.center.z) * pixelsPerWorldUnit
            );
            CreatePlayerMarker(playerPosLocal);
        }
        else
        {
            // --- COMPORTAMIENTO NORMAL DEL ROGUELITE ---
            float collapsedPPU = Mathf.Clamp(collapsedPixelsPerWorldUnit, minPixelsPerWorldUnit, maxPixelsPerWorldUnit);
            float expandedPPU = CalculateExpandedAreaScale(discoveredAreas, playerPosition);

            float expansionT = 0f;
            if (allowTemporaryExpand && expandedScale > 1.01f)
            {
                expansionT = Mathf.Clamp01((currentExpandScale - 1f) / (expandedScale - 1f));
            }

            float pixelsPerWorldUnit = Mathf.Lerp(collapsedPPU, expandedPPU, expansionT);

            for (int i = 0; i < discoveredAreas.Count; i++)
            {
                RoomInstance area = discoveredAreas[i];
                IReadOnlyList<Bounds> segments = area.VisualSegments;

                for (int segmentIndex = 0; segmentIndex < segments.Count; segmentIndex++)
                {
                    Bounds segment = segments[segmentIndex];
                    Vector2 localCenter = new Vector2(
                        (segment.center.x - playerPosition.x) * pixelsPerWorldUnit,
                        (segment.center.z - playerPosition.z) * pixelsPerWorldUnit
                    );

                    Vector2 visualSize = GetAreaVisualSize(area, segment, pixelsPerWorldUnit, false);
                    CreateMapBlock(localCenter, visualSize, GetAreaColor(area));
                }
            }

            CreatePlayerMarker(Vector2.zero); // En modo normal siempre en el centro de la UI
        }

        return true;
    }

    private List<RoomInstance> CollectDiscoveredAreas()
    {
        List<RoomInstance> discoveredAreas = new List<RoomInstance>();
        IReadOnlyList<RoomInstance> areas = RoomInstance.ActiveInstances;
        bool isTutorial = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level_Tuto";

        for (int i = 0; i < areas.Count; i++)
        {
            RoomInstance area = areas[i];
            if (area != null && (area.IsDiscovered || isTutorial) && area.HasBounds)
            {
                discoveredAreas.Add(area);
            }
        }

        return discoveredAreas;
    }

    private float CalculateExpandedAreaScale(List<RoomInstance> discoveredAreas, Vector3 playerPosition)
    {
        Vector2 availableSize = GetCurrentPanelSize() - new Vector2(mapPadding * 2f, mapPadding * 2f);
        float halfWidth = Mathf.Max(1f, availableSize.x * 0.5f);
        float halfHeight = Mathf.Max(1f, availableSize.y * 0.5f);
        float maxAbsX = 1f;
        float maxAbsZ = 1f;

        IReadOnlyList<RoomInstance> areasToConsider = RoomInstance.ActiveInstances;
        if (areasToConsider.Count == 0)
        {
            areasToConsider = discoveredAreas; 
        }

        for (int i = 0; i < areasToConsider.Count; i++)
        {
            RoomInstance area = areasToConsider[i];
            if (area == null || !area.HasBounds) continue;

            IReadOnlyList<Bounds> segments = area.VisualSegments;
            for (int segmentIndex = 0; segmentIndex < segments.Count; segmentIndex++)
            {
                Bounds segment = segments[segmentIndex];
                maxAbsX = Mathf.Max(maxAbsX, Mathf.Abs(segment.min.x - playerPosition.x));
                maxAbsX = Mathf.Max(maxAbsX, Mathf.Abs(segment.max.x - playerPosition.x));
                maxAbsZ = Mathf.Max(maxAbsZ, Mathf.Abs(segment.min.z - playerPosition.z));
                maxAbsZ = Mathf.Max(maxAbsZ, Mathf.Abs(segment.max.z - playerPosition.z));
            }
        }

        float scaleX = halfWidth / maxAbsX;
        float scaleY = halfHeight / maxAbsZ;
        return Mathf.Clamp(Mathf.Min(scaleX, scaleY), GetExpandedMinimumPixelsPerWorldUnit(), maxPixelsPerWorldUnit);
    }

    private void DrawExplorationFallback()
    {
        if (!hasCurrentExplorationCell)
        {
            UpdateExplorationFallback();
        }

        if (!hasCurrentExplorationCell)
        {
            return;
        }

        float collapsedPPU = Mathf.Clamp(collapsedPixelsPerWorldUnit, minPixelsPerWorldUnit, maxPixelsPerWorldUnit);
        float expandedPPU = CalculateExpandedFallbackScale();

        float expansionT = 0f;
        if (allowTemporaryExpand && expandedScale > 1.01f)
        {
            expansionT = Mathf.Clamp01((currentExpandScale - 1f) / (expandedScale - 1f));
        }

        float pixelsPerWorldUnit = Mathf.Lerp(collapsedPPU, expandedPPU, expansionT);

        int explorationQuarterTurns = GetExplorationQuarterTurns();

        foreach (Vector2Int exploredCell in exploredCells)
        {
            Vector2Int deltaCell = exploredCell - currentExplorationCell;
            Vector2Int rotatedDelta = RotateGrid(deltaCell, explorationQuarterTurns);
            Vector2 cellCenter = new Vector2(
                rotatedDelta.x * explorationCellWorldSize * pixelsPerWorldUnit,
                rotatedDelta.y * explorationCellWorldSize * pixelsPerWorldUnit
            );

            float cellSide = Mathf.Max(minimumRoomSide * 0.75f, explorationCellWorldSize * pixelsPerWorldUnit);
            CreateMapBlock(cellCenter, new Vector2(cellSide, cellSide), normalRoomColor);
        }

        CreatePlayerMarker(Vector2.zero);
    }

    private float CalculateExpandedFallbackScale()
    {
        Vector2 availableSize = GetCurrentPanelSize() - new Vector2(mapPadding * 2f, mapPadding * 2f);
        float halfWidth = Mathf.Max(1f, availableSize.x * 0.5f);
        float halfHeight = Mathf.Max(1f, availableSize.y * 0.5f);
        float maxAbsX = 1f;
        float maxAbsY = 1f;
        int explorationQuarterTurns = GetExplorationQuarterTurns();

        foreach (Vector2Int exploredCell in exploredCells)
        {
            Vector2Int deltaCell = exploredCell - currentExplorationCell;
            Vector2Int rotatedDelta = RotateGrid(deltaCell, explorationQuarterTurns);
            float worldX = Mathf.Abs(rotatedDelta.x * explorationCellWorldSize) + (explorationCellWorldSize * 0.5f);
            float worldY = Mathf.Abs(rotatedDelta.y * explorationCellWorldSize) + (explorationCellWorldSize * 0.5f);

            maxAbsX = Mathf.Max(maxAbsX, worldX);
            maxAbsY = Mathf.Max(maxAbsY, worldY);
        }

        float scaleX = halfWidth / maxAbsX;
        float scaleY = halfHeight / maxAbsY;
        return Mathf.Clamp(Mathf.Min(scaleX, scaleY), GetExpandedMinimumPixelsPerWorldUnit(), maxPixelsPerWorldUnit);
    }

    private Vector2 GetAreaVisualSize(RoomInstance area, Bounds bounds, float pixelsPerWorldUnit, bool bypassLimits)
    {
        if (bypassLimits)
        {
            // Evitar distorsiones en el tutorial, usando la escala real estricta.
            return new Vector2(bounds.size.x * pixelsPerWorldUnit, bounds.size.z * pixelsPerWorldUnit);
        }

        if (area.AreaShape == MapAreaShape.Corridor)
        {
            float width = Mathf.Max(minimumCorridorThickness, bounds.size.x * pixelsPerWorldUnit);
            float height = Mathf.Max(minimumCorridorThickness, bounds.size.z * pixelsPerWorldUnit);
            return new Vector2(width, height);
        }

        float roomSide = Mathf.Max(minimumRoomSide, Mathf.Max(bounds.size.x, bounds.size.z) * pixelsPerWorldUnit);
        return new Vector2(roomSide, roomSide);
    }

    private Color GetAreaColor(RoomInstance area)
    {
        if (area.AreaShape == MapAreaShape.Corridor)
        {
            return area.IsCurrentArea ? currentCorridorColor : corridorColor;
        }

        if (area.IsCurrentArea)
        {
            return currentRoomColor;
        }

        switch (area.RoomType)
        {
            case DungeonRoomType.Start:
                return startRoomColor;
            case DungeonRoomType.Final:
                return finalRoomColor;
            default:
                return normalRoomColor;
        }
    }

    private void CreateMapBlock(Vector2 anchoredPosition, Vector2 size, Color color)
    {
        GameObject roomObject = new GameObject("MapBlock", typeof(RectTransform), typeof(Image));
        roomObject.transform.SetParent(roomsRoot, false);

        RectTransform rectTransform = roomObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        Image image = roomObject.GetComponent<Image>();
        image.sprite = GetWhiteSprite();
        image.color = color;
        image.raycastTarget = false;
    }

    private void CreatePlayerMarker(Vector2 anchoredPosition)
    {
        GameObject markerObject = new GameObject("PlayerMarker", typeof(RectTransform), typeof(Image));
        markerObject.transform.SetParent(roomsRoot, false);

        RectTransform rectTransform = markerObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = GetSquareSize(currentMarkerSize);

        Image image = markerObject.GetComponent<Image>();
        image.sprite = GetWhiteSprite();
        image.color = currentMarkerColor;
        image.raycastTarget = false;
    }

    private void EnsureUiRoots()
    {
        if (contentRoot == null)
        {
            contentRoot = transform as RectTransform;
        }

        if (contentRoot == null)
        {
            Debug.LogWarning("MapUI requiere un RectTransform para dibujar el minimapa.");
            return;
        }

        containerRoot = contentRoot.parent as RectTransform;

        if (autoConfigureContainer && containerRoot != null)
        {
            ConfigureContainer();
        }

        ConfigureContentRoot();
        ApplyContainerSize();

        if (roomsRoot == null)
        {
            roomsRoot = GetOrCreateLayer("Map");
        }
    }

    private void ConfigureContainer()
    {
        containerRoot.anchorMin = new Vector2(1f, 1f);
        containerRoot.anchorMax = new Vector2(1f, 1f);
        containerRoot.pivot = new Vector2(1f, 1f);
        containerRoot.anchoredPosition = new Vector2(-panelMargin.x, -panelMargin.y);
        containerRoot.localRotation = Quaternion.identity;
        containerRoot.localScale = Vector3.one;
        ApplyContainerSize();
    }

    private void ConfigureContentRoot()
    {
        contentRoot.anchorMin = Vector2.zero;
        contentRoot.anchorMax = Vector2.one;
        contentRoot.pivot = new Vector2(0.5f, 0.5f);
        contentRoot.anchoredPosition = Vector2.zero;
        contentRoot.sizeDelta = Vector2.zero;
        contentRoot.localScale = Vector3.one;
        contentRoot.localRotation = Quaternion.identity;

        Image backgroundImage = contentRoot.GetComponent<Image>();
        if (backgroundImage != null)
        {
            backgroundImage.raycastTarget = false;
        }
    }

    private void ApplyContainerSize()
    {
        if (containerRoot == null)
        {
            return;
        }

        float safeScale = Mathf.Max(1f, currentExpandScale);
        containerRoot.sizeDelta = panelSize * safeScale;
    }

    private Vector2 GetCurrentPanelSize()
    {
        return panelSize * Mathf.Max(1f, currentExpandScale);
    }

    private bool IsExpandedView()
    {
        return allowTemporaryExpand && currentExpandScale > 1.01f;
    }

    private RectTransform GetOrCreateLayer(string layerName)
    {
        Transform existingLayer = contentRoot.Find(layerName);
        RectTransform layer = existingLayer as RectTransform;

        if (layer == null)
        {
            GameObject layerObject = new GameObject(layerName, typeof(RectTransform));
            layerObject.transform.SetParent(contentRoot, false);
            layer = layerObject.GetComponent<RectTransform>();
            layer.anchorMin = new Vector2(0.5f, 0.5f);
            layer.anchorMax = new Vector2(0.5f, 0.5f);
            layer.pivot = new Vector2(0.5f, 0.5f);
            layer.anchoredPosition = Vector2.zero;
            layer.sizeDelta = Vector2.zero;
        }

        return layer;
    }

    private float GetExpandedMinimumPixelsPerWorldUnit()
    {
        return Mathf.Max(0.1f, Mathf.Min(expandedMinPixelsPerWorldUnit, maxPixelsPerWorldUnit));
    }

    private int GetExplorationQuarterTurns()
    {
        if (hasExplorationOrientation)
        {
            return explorationQuarterTurnsToUp;
        }

        if (playerTransform != null)
        {
            Vector2 forward = new Vector2(playerTransform.forward.x, playerTransform.forward.z);
            if (forward.sqrMagnitude > 0.01f)
            {
                Vector2Int forwardGrid = GetDominantGridDirection(forward);
                if (forwardGrid != Vector2Int.zero)
                {
                    return GetQuarterTurnsToUp(forwardGrid);
                }
            }
        }

        return 0;
    }

    private int GetQuarterTurnsToUp(Vector2Int direction)
    {
        if (direction == Vector2Int.zero)
        {
            return 0;
        }

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            return direction.x > 0 ? 1 : 3;
        }

        return direction.y >= 0 ? 0 : 2;
    }

    private static Vector2Int GetDominantGridDirection(Vector2 direction)
    {
        if (direction == Vector2.zero)
        {
            return Vector2Int.zero;
        }

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            return new Vector2Int(direction.x >= 0f ? 1 : -1, 0);
        }

        return new Vector2Int(0, direction.y >= 0f ? 1 : -1);
    }

    private static Vector2Int RotateGrid(Vector2Int point, int quarterTurns)
    {
        int normalizedTurns = ((quarterTurns % 4) + 4) % 4;

        switch (normalizedTurns)
        {
            case 1:
                return new Vector2Int(-point.y, point.x);
            case 2:
                return new Vector2Int(-point.x, -point.y);
            case 3:
                return new Vector2Int(point.y, -point.x);
            default:
                return point;
        }
    }

    private static Vector2 GetSquareSize(Vector2 size)
    {
        float side = Mathf.Max(size.x, size.y);
        return new Vector2(side, side);
    }

    private Vector2Int WorldToExplorationCell(Vector3 worldPosition)
    {
        if (Mathf.Approximately(explorationCellWorldSize, 0f))
        {
            return Vector2Int.zero;
        }

        return new Vector2Int(
            Mathf.RoundToInt(worldPosition.x / explorationCellWorldSize),
            Mathf.RoundToInt(worldPosition.z / explorationCellWorldSize)
        );
    }

    private static void ClearChildren(RectTransform root)
    {
        if (root == null)
        {
            return;
        }

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            GameObject child = root.GetChild(i).gameObject;
            child.SetActive(false);
            UnityEngine.Object.Destroy(child);
        }
    }

    private static Sprite GetWhiteSprite()
    {
        if (cachedWhiteSprite == null)
        {
            cachedWhiteSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
                new Vector2(0.5f, 0.5f)
            );
        }

        return cachedWhiteSprite;
    }
}