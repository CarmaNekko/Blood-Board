using System.Collections.Generic;
using UnityEngine;

public class RoomVisibilityManager : MonoBehaviour
{
    private static RoomVisibilityManager instance;
    private static readonly List<RoomInstance> allRooms = new List<RoomInstance>();
    private static readonly List<GameObject> allDestructibleObjects = new List<GameObject>();
    private static Transform playerTransform;
    private static DungeonLayout currentLayout;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (instance != null) return;
        
        GameObject go = new GameObject("RoomVisibilityManager");
        instance = go.AddComponent<RoomVisibilityManager>();
        DontDestroyOnLoad(go);
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        Vector3 playerPos = playerTransform != null ? playerTransform.position : Vector3.zero;

        for (int i = allRooms.Count - 1; i >= 0; i--)
        {
            RoomInstance room = allRooms[i];
            if (room == null || room.gameObject == null)
            {
                allRooms.RemoveAt(i);
                continue;
            }

            bool isActive = ShouldRoomBeActive(room);
            room.SetObjectsActive(isActive);
        }

        for (int i = allDestructibleObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = allDestructibleObjects[i];
            if (obj == null)
            {
                allDestructibleObjects.RemoveAt(i);
                continue;
            }

            Renderer rend = obj.GetComponent<Renderer>();
            if (rend != null)
            {
                bool shouldBeActive = ShouldDestructibleBeActive(obj, playerPos);
                if (rend.enabled != shouldBeActive)
                {
                    rend.enabled = shouldBeActive;
                }
            }
        }
    }

    private static bool ShouldDestructibleBeActive(GameObject destructible, Vector3 playerPos)
    {
        if (currentLayout == null)
        {
            return true;
        }

        Vector3 destructiblePos = destructible.transform.position;
        float minDistSqr = float.MaxValue;
        
        foreach (RoomInstance room in allRooms)
        {
            if (room == null || room.gameObject == null) continue;
            
            if (!room.IsCurrentArea && !AreRoomsConnected(currentLayout.CurrentRoomId, room.RoomId))
            {
                continue;
            }
            
            Vector3 roomCenter = room.HasBounds ? room.WorldBounds.center : room.transform.position;
            float distSqr = Vector3.SqrMagnitude(destructiblePos - roomCenter);
            if (distSqr < minDistSqr) minDistSqr = distSqr;
        }
        
        float roomRadius = 35f;
        RoomInstance currentRoom = GetRoomById(currentLayout.CurrentRoomId);
        if (currentRoom != null && currentRoom.HasBounds)
        {
            roomRadius = Mathf.Max(currentRoom.WorldBounds.extents.x, currentRoom.WorldBounds.extents.z) + 20f;
        }
        
        return minDistSqr <= roomRadius * roomRadius;
    }

    private static RoomInstance GetRoomById(int roomId)
    {
        foreach (RoomInstance room in allRooms)
        {
            if (room.RoomId == roomId) return room;
        }
        return null;
    }

    private static bool ShouldRoomBeActive(RoomInstance room)
    {
        if (playerTransform == null)
        {
            return true;
        }

        if (room.IsCurrentArea)
        {
            return true;
        }

        if (currentLayout != null && room.RoomId >= 0)
        {
            if (currentLayout.CurrentRoomId >= 0)
            {
                if (currentLayout.CurrentRoomId == room.RoomId)
                {
                    return true;
                }
                
                if (AreRoomsConnected(currentLayout.CurrentRoomId, room.RoomId))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool AreRoomsConnected(int roomAId, int roomBId)
    {
        if (currentLayout == null) return false;
        
        if (currentLayout.TryGetRoom(roomAId, out var roomA))
        {
            return roomA.neighbors.Contains(roomBId);
        }
        
        return false;
    }

    public static void SetLayout(DungeonLayout layout)
    {
        currentLayout = layout;
    }

    public static void RegisterRoom(RoomInstance room)
    {
        if (instance == null) Initialize();
        
        if (!allRooms.Contains(room))
        {
            allRooms.Add(room);
        }
        
        if (ShouldRoomBeActive(room))
        {
            room.SetObjectsActive(true);
        }
    }

    public static void UnregisterRoom(RoomInstance room)
    {
        allRooms.Remove(room);
    }

    public static void RegisterDestructible(GameObject destructible)
    {
        if (instance == null) Initialize();
        
        if (!allDestructibleObjects.Contains(destructible))
        {
            allDestructibleObjects.Add(destructible);
        }
    }

    public static void UnregisterDestructible(GameObject destructible)
    {
        allDestructibleObjects.Remove(destructible);
    }

    public static void SetPlayer(Transform player)
    {
        playerTransform = player;
    }
}