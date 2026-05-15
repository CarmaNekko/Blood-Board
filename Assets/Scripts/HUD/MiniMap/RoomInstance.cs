using System;
using System.Collections.Generic;
using UnityEngine;

public enum MapAreaShape
{
    Room,
    Corridor,
    Generic
}

[DisallowMultipleComponent]
public class RoomInstance : MonoBehaviour
{
    public static event Action AreasChanged;

    private static readonly List<RoomInstance> activeInstances = new List<RoomInstance>();
    private static RoomInstance currentAreaInstance;

    [Header("Discovery")]
    [SerializeField] private float discoveryPadding = 2f;
    [SerializeField] private float fallbackRadius = 10f;
    [SerializeField] private MapAreaShape areaShape = MapAreaShape.Room;
    [SerializeField] private float corridorVisualThicknessWorld = 5f;
    [SerializeField] private int walkableLayer = 6;

    public static IReadOnlyList<RoomInstance> ActiveInstances => activeInstances;

    public int RoomId { get; private set; } = -1;
    public DungeonRoomType RoomType { get; private set; } = DungeonRoomType.Normal;
    public bool IsDiscovered { get; private set; }
    public bool IsCurrentArea => currentAreaInstance == this;
    public MapAreaShape AreaShape => areaShape;
    public bool HasBounds => hasCachedBounds;
    public Bounds WorldBounds => cachedBounds;
    public IReadOnlyList<Bounds> VisualSegments => visualSegments;

    private readonly List<Bounds> visualSegments = new List<Bounds>();

    private DungeonLayout layout;
    private Transform playerTransform;
    private Bounds cachedBounds;
    private bool hasCachedBounds;

    public void Initialize(
        DungeonLayout dungeonLayout,
        int roomId,
        DungeonRoomType roomType,
        MapAreaShape shape,
        bool discoverOnStart)
    {
        layout = dungeonLayout;
        RoomId = roomId;
        RoomType = roomType;
        areaShape = shape;

        CacheBounds();
        layout?.BindRoomInstance(roomId, this);

        if (discoverOnStart)
        {
            MarkDiscovered();
            SetAsCurrentArea();
        }
    }

    public void InitializeStandalone(MapAreaShape shape, bool discoverOnStart)
    {
        layout = null;
        RoomId = -1;
        RoomType = DungeonRoomType.Normal;
        areaShape = shape;

        CacheBounds();

        if (discoverOnStart)
        {
            MarkDiscovered();
            SetAsCurrentArea();
        }
    }

    private void OnEnable()
    {
        if (!activeInstances.Contains(this))
        {
            activeInstances.Add(this);
            AreasChanged?.Invoke();
        }
    }

    private void OnDisable()
    {
        if (activeInstances.Remove(this))
        {
            if (currentAreaInstance == this)
            {
                currentAreaInstance = null;
            }

            AreasChanged?.Invoke();
        }
    }

    private void Awake()
    {
        CacheBounds();
    }

    private void Update()
    {
        // Check if room should be discovered based on layout
        if (!IsDiscovered && layout != null && RoomId >= 0 && layout.IsDiscovered(RoomId))
        {
            MarkDiscovered();
            AreasChanged?.Invoke();
        }

        if (playerTransform == null)
        {
            ResolvePlayer();
        }

        if (playerTransform == null)
        {
            return;
        }

        if (!IsPlayerInsideArea(playerTransform.position))
        {
            return;
        }

        bool changed = false;

        if (MarkDiscovered())
        {
            changed = true;
            RoomEnemySpawner spawner = GetComponent<RoomEnemySpawner>();
            if (spawner != null) spawner.TriggerRoomEvent();
        }

        if (SetAsCurrentArea())
        {
            changed = true;
        }

        if (changed)
        {
            AreasChanged?.Invoke();
        }
    }

    private void ResolvePlayer()
    {
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

    private bool IsPlayerInsideArea(Vector3 playerPosition)
    {
        if (hasCachedBounds)
        {
            Bounds expandedBounds = cachedBounds;
            expandedBounds.Expand(new Vector3(discoveryPadding * 2f, 6f, discoveryPadding * 2f));
            return expandedBounds.Contains(playerPosition);
        }

        Vector2 areaCenter = new Vector2(transform.position.x, transform.position.z);
        Vector2 playerPoint = new Vector2(playerPosition.x, playerPosition.z);
        return Vector2.Distance(areaCenter, playerPoint) <= fallbackRadius;
    }

    private bool MarkDiscovered()
    {
        if (IsDiscovered)
        {
            return false;
        }

        IsDiscovered = true;

        if (layout != null && RoomId >= 0)
        {
            layout.DiscoverRoom(RoomId);
        }

        return true;
    }

    private bool SetAsCurrentArea()
    {
        if (currentAreaInstance == this)
        {
            if (layout != null && RoomId >= 0)
            {
                layout.SetCurrentRoom(RoomId);
            }

            return false;
        }

        currentAreaInstance = this;

        if (layout != null && RoomId >= 0)
        {
            layout.SetCurrentRoom(RoomId);
        }

        return true;
    }

    private void CacheBounds()
    {
        if (TryBuildWalkableBounds())
        {
            return;
        }

        Collider[] colliders = GetComponentsInChildren<Collider>();
        if (TryBuildBoundsFromColliders(colliders))
        {
            BuildFallbackVisualSegments();
            return;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        hasCachedBounds = TryBuildBoundsFromRenderers(renderers);
        BuildFallbackVisualSegments();
    }

    private bool TryBuildBoundsFromColliders(Collider[] colliders)
    {
        hasCachedBounds = false;

        for (int i = 0; i < colliders.Length; i++)
        {
            if (!colliders[i].enabled)
            {
                continue;
            }

            if (!hasCachedBounds)
            {
                cachedBounds = colliders[i].bounds;
                hasCachedBounds = true;
            }
            else
            {
                cachedBounds.Encapsulate(colliders[i].bounds);
            }
        }

        return hasCachedBounds;
    }

    private bool TryBuildBoundsFromRenderers(Renderer[] renderers)
    {
        bool foundRenderer = false;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (!renderers[i].enabled)
            {
                continue;
            }

            if (!foundRenderer)
            {
                cachedBounds = renderers[i].bounds;
                foundRenderer = true;
            }
            else
            {
                cachedBounds.Encapsulate(renderers[i].bounds);
            }
        }

        hasCachedBounds = foundRenderer;
        return foundRenderer;
    }

    private bool TryBuildWalkableBounds()
    {
        List<Bounds> walkableBounds = CollectWalkableBounds();
        if (walkableBounds.Count == 0)
        {
            return false;
        }

        hasCachedBounds = false;
        visualSegments.Clear();

        List<Bounds> segments = areaShape == MapAreaShape.Corridor
            ? MergeCorridorSegments(walkableBounds)
            : walkableBounds;

        for (int i = 0; i < segments.Count; i++)
        {
            Bounds segment = segments[i];
            if (!hasCachedBounds)
            {
                cachedBounds = segment;
                hasCachedBounds = true;
            }
            else
            {
                cachedBounds.Encapsulate(segment);
            }
        }

        if (!hasCachedBounds)
        {
            return false;
        }

        if (areaShape == MapAreaShape.Corridor)
        {
            visualSegments.AddRange(segments);
        }
        else
        {
            visualSegments.Add(cachedBounds);
        }

        return true;
    }

    private List<Bounds> CollectWalkableBounds()
    {
        List<Bounds> walkableBounds = new List<Bounds>();
        Collider[] colliders = GetComponentsInChildren<Collider>();

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (!collider.enabled || collider.gameObject.layer != walkableLayer)
            {
                continue;
            }

            walkableBounds.Add(collider.bounds);
        }

        if (walkableBounds.Count > 0)
        {
            return walkableBounds;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (!renderer.enabled || renderer.gameObject.layer != walkableLayer)
            {
                continue;
            }

            walkableBounds.Add(renderer.bounds);
        }

        return walkableBounds;
    }

    private static List<Bounds> MergeCorridorSegments(List<Bounds> sourceBounds)
    {
        List<Bounds> mergedBounds = new List<Bounds>(sourceBounds);
        bool mergedAny = true;

        while (mergedAny)
        {
            mergedAny = false;

            for (int i = 0; i < mergedBounds.Count; i++)
            {
                for (int j = i + 1; j < mergedBounds.Count; j++)
                {
                    if (!CanMergeCorridorSegments(mergedBounds[i], mergedBounds[j]))
                    {
                        continue;
                    }

                    Bounds combined = mergedBounds[i];
                    combined.Encapsulate(mergedBounds[j]);
                    mergedBounds[i] = combined;
                    mergedBounds.RemoveAt(j);
                    mergedAny = true;
                    break;
                }

                if (mergedAny)
                {
                    break;
                }
            }
        }

        return mergedBounds;
    }

    private static bool CanMergeCorridorSegments(Bounds first, Bounds second)
    {
        const float overlapTolerance = 0.15f;
        const float profileTolerance = 0.35f;

        if (!BoundsTouchOrOverlapXZ(first, second, overlapTolerance))
        {
            return false;
        }

        return HaveSimilarXProfile(first, second, profileTolerance) ||
               HaveSimilarZProfile(first, second, profileTolerance);
    }

    private static bool BoundsTouchOrOverlapXZ(Bounds first, Bounds second, float tolerance)
    {
        return first.min.x <= second.max.x + tolerance &&
               first.max.x >= second.min.x - tolerance &&
               first.min.z <= second.max.z + tolerance &&
               first.max.z >= second.min.z - tolerance;
    }

    private static bool HaveSimilarXProfile(Bounds first, Bounds second, float tolerance)
    {
        return Mathf.Abs(first.center.x - second.center.x) <= tolerance &&
               Mathf.Abs(first.size.x - second.size.x) <= tolerance * 2f;
    }

    private static bool HaveSimilarZProfile(Bounds first, Bounds second, float tolerance)
    {
        return Mathf.Abs(first.center.z - second.center.z) <= tolerance &&
               Mathf.Abs(first.size.z - second.size.z) <= tolerance * 2f;
    }

    private void BuildFallbackVisualSegments()
    {
        visualSegments.Clear();

        if (!hasCachedBounds)
        {
            return;
        }

        if (areaShape != MapAreaShape.Corridor)
        {
            visualSegments.Add(cachedBounds);
            return;
        }

        DoorConnector[] doors = GetComponentsInChildren<DoorConnector>();
        if (doors.Length >= 2 && TryBuildCorridorSegments(doors[0], doors[1]))
        {
            return;
        }

        visualSegments.Add(cachedBounds);
    }

    private bool TryBuildCorridorSegments(DoorConnector firstDoor, DoorConnector secondDoor)
    {
        Vector3 firstForward = firstDoor.transform.forward;
        Vector3 secondForward = secondDoor.transform.forward;
        bool firstIsHorizontalEdge = Mathf.Abs(firstForward.x) > Mathf.Abs(firstForward.z);
        bool secondIsHorizontalEdge = Mathf.Abs(secondForward.x) > Mathf.Abs(secondForward.z);

        if (firstIsHorizontalEdge == secondIsHorizontalEdge)
        {
            visualSegments.Add(BuildStraightCorridorSegment(firstDoor.transform.position, secondDoor.transform.position));
            return true;
        }

        DoorConnector horizontalDoor = firstIsHorizontalEdge ? firstDoor : secondDoor;
        DoorConnector verticalDoor = firstIsHorizontalEdge ? secondDoor : firstDoor;

        Vector3 elbow = new Vector3(
            verticalDoor.transform.position.x,
            cachedBounds.center.y,
            horizontalDoor.transform.position.z
        );

        float thickness = corridorVisualThicknessWorld;
        visualSegments.Add(BuildHorizontalSegment(horizontalDoor.transform.position, elbow, thickness));
        visualSegments.Add(BuildVerticalSegment(verticalDoor.transform.position, elbow, thickness));
        return true;
    }

    private Bounds BuildStraightCorridorSegment(Vector3 firstDoorPosition, Vector3 secondDoorPosition)
    {
        float dx = Mathf.Abs(firstDoorPosition.x - secondDoorPosition.x);
        float dz = Mathf.Abs(firstDoorPosition.z - secondDoorPosition.z);
        float thickness = corridorVisualThicknessWorld;

        if (dx >= dz)
        {
            return BuildHorizontalSegment(firstDoorPosition, secondDoorPosition, thickness);
        }

        return BuildVerticalSegment(firstDoorPosition, secondDoorPosition, thickness);
    }

    private Bounds BuildHorizontalSegment(Vector3 start, Vector3 end, float thickness)
    {
        float length = Mathf.Max(thickness, Mathf.Abs(start.x - end.x));
        Vector3 center = new Vector3((start.x + end.x) * 0.5f, cachedBounds.center.y, (start.z + end.z) * 0.5f);
        Vector3 size = new Vector3(length, cachedBounds.size.y, thickness);
        return new Bounds(center, size);
    }

    private Bounds BuildVerticalSegment(Vector3 start, Vector3 end, float thickness)
    {
        float length = Mathf.Max(thickness, Mathf.Abs(start.z - end.z));
        Vector3 center = new Vector3((start.x + end.x) * 0.5f, cachedBounds.center.y, (start.z + end.z) * 0.5f);
        Vector3 size = new Vector3(thickness, cachedBounds.size.y, length);
        return new Bounds(center, size);
    }
}
