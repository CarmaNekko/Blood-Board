using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class ModularGenerator : MonoBehaviour
{
    [Header("Habitaciones")]
    public GameObject startRoomPrefab;
    public GameObject finalRoomPrefab;
    public List<GameObject> roomPrefabs;

    [Header("Pasillos")]
    public List<GameObject> corridorPrefabs;

    [Header("Parches")]
    public List<GameObject> deadEndPrefabs;

    [Header("Configuracion")]
    public int maxRooms = 6;
    public LayerMask collisionMask;
    public NavMeshSurface navMesh;
    [SerializeField] private float mapGridWorldSize = 30f;

    private readonly List<DoorConnector> pendingRoomDoors = new List<DoorConnector>();
    private readonly List<DoorConnector> pendingCorridorDoors = new List<DoorConnector>();
    private readonly List<GameObject> allSpawnedPieces = new List<GameObject>();

    private int roomCount;
    private DungeonLayout generatedLayout;

    public DungeonLayout GenerateLevel(int targetRooms)
    {
        maxRooms = targetRooms;
        pendingRoomDoors.Clear();
        pendingCorridorDoors.Clear();
        allSpawnedPieces.Clear();
        roomCount = 0;
        generatedLayout = new DungeonLayout();

        GameObject startRoom = Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        allSpawnedPieces.Add(startRoom);
        roomCount++;

        int startRoomId = RegisterRoom(startRoom, DungeonRoomType.Start, true);
        AddDoorsToList(startRoom, false, startRoomId, startRoomId);

        int attempts = 0;
        while (roomCount < maxRooms &&
               (pendingRoomDoors.Count > 0 || pendingCorridorDoors.Count > 0) &&
               attempts < 2000)
        {
            attempts++;
            SpawnNextPiece();
        }

        PlaceFinalRoom();
        SealOpenDoors();

        if (navMesh != null)
        {
            navMesh.BuildNavMesh();
        }

        Debug.Log($"Calabozo generado. Habitaciones: {roomCount}. Intentos: {attempts}");
        return generatedLayout;
    }

    private void SpawnNextPiece()
    {
        if (pendingCorridorDoors.Count > 0)
        {
            DoorConnector targetDoor = TakeRandomPendingDoor(pendingCorridorDoors);
            if (targetDoor != null)
            {
                TryConnect(targetDoor, roomPrefabs, false);
            }
        }
        else if (pendingRoomDoors.Count > 0)
        {
            DoorConnector targetDoor = TakeRandomPendingDoor(pendingRoomDoors);
            if (targetDoor != null)
            {
                TryConnect(targetDoor, corridorPrefabs, true);
            }
        }
    }

    private bool TryConnect(DoorConnector targetDoor, List<GameObject> prefabList, bool isCorridor)
    {
        if (targetDoor == null || targetDoor.isConnected)
        {
            return false;
        }

        List<GameObject> shuffled = new List<GameObject>(prefabList);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffled.Count);
            GameObject temp = shuffled[i];
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }

        foreach (GameObject prefab in shuffled)
        {
            GameObject newPiece = Instantiate(prefab);
            DoorConnector[] newDoors = newPiece.GetComponentsInChildren<DoorConnector>();

            foreach (DoorConnector newDoor in newDoors)
            {
                if (newDoor.doorHeightOffset != targetDoor.doorHeightOffset)
                {
                    continue;
                }

                AlignPiece(targetDoor, newDoor, newPiece);

                if (HasOverlap(newPiece))
                {
                    continue;
                }

                allSpawnedPieces.Add(newPiece);
                newDoor.isConnected = true;
                targetDoor.isConnected = true;

                if (isCorridor)
                {
                    RegisterStandaloneArea(newPiece, MapAreaShape.Corridor, false);
                    AddDoorsToList(newPiece, true, -1, targetDoor.GetSourceRoomNodeId());
                }
                else
                {
                    roomCount++;
                    int newRoomId = RegisterRoom(newPiece, DungeonRoomType.Normal, false);
                    int parentRoomId = targetDoor.GetSourceRoomNodeId();
                    if (parentRoomId >= 0)
                    {
                        generatedLayout.ConnectRooms(parentRoomId, newRoomId);
                    }

                    AddDoorsToList(newPiece, false, newRoomId, newRoomId);
                }

                return true;
            }

            newPiece.SetActive(false);
            Destroy(newPiece);
        }

        return false;
    }

    private void PlaceFinalRoom()
    {
        List<DoorConnector> candidates = new List<DoorConnector>(pendingCorridorDoors);
        candidates.AddRange(pendingRoomDoors);

        for (int i = candidates.Count - 1; i >= 0; i--)
        {
            DoorConnector targetDoor = candidates[i];
            if (targetDoor == null || targetDoor.isConnected)
            {
                continue;
            }

            GameObject finalRoom = Instantiate(finalRoomPrefab);
            DoorConnector finalDoor = finalRoom.GetComponentInChildren<DoorConnector>();

            if (finalDoor.doorHeightOffset != targetDoor.doorHeightOffset)
            {
                finalRoom.SetActive(false);
                Destroy(finalRoom);
                continue;
            }

            AlignPiece(targetDoor, finalDoor, finalRoom);

            if (HasOverlap(finalRoom))
            {
                finalRoom.SetActive(false);
                Destroy(finalRoom);
                continue;
            }

            allSpawnedPieces.Add(finalRoom);
            targetDoor.isConnected = true;
            finalDoor.isConnected = true;

            int finalRoomId = RegisterRoom(finalRoom, DungeonRoomType.Final, false);
            int parentRoomId = targetDoor.GetSourceRoomNodeId();
            if (parentRoomId >= 0)
            {
                generatedLayout.ConnectRooms(parentRoomId, finalRoomId);
            }

            return;
        }
    }

    private void SealOpenDoors()
    {
        foreach (DoorConnector corridorDoor in pendingCorridorDoors)
        {
            if (corridorDoor != null && !corridorDoor.isConnected)
            {
                PlaceDeadEnd(corridorDoor);
            }
        }

        foreach (DoorConnector roomDoor in pendingRoomDoors)
        {
            if (roomDoor == null || roomDoor.isConnected)
            {
                continue;
            }

            bool corridorPlaced = false;

            foreach (GameObject corridorPrefab in corridorPrefabs)
            {
                GameObject corridor = Instantiate(corridorPrefab);
                DoorConnector corridorEntrance = corridor.GetComponentInChildren<DoorConnector>();

                if (corridorEntrance.doorHeightOffset != roomDoor.doorHeightOffset)
                {
                    corridor.SetActive(false);
                    Destroy(corridor);
                    continue;
                }

                AlignPiece(roomDoor, corridorEntrance, corridor);

                if (HasOverlap(corridor))
                {
                    corridor.SetActive(false);
                    Destroy(corridor);
                    continue;
                }

                allSpawnedPieces.Add(corridor);
                RegisterStandaloneArea(corridor, MapAreaShape.Corridor, false);
                roomDoor.isConnected = true;
                corridorEntrance.isConnected = true;
                corridorPlaced = true;

                foreach (DoorConnector corridorExit in corridor.GetComponentsInChildren<DoorConnector>())
                {
                    if (!corridorExit.isConnected)
                    {
                        PlaceDeadEnd(corridorExit);
                    }
                }

                break;
            }

            if (!corridorPlaced)
            {
                PlaceDeadEnd(roomDoor);
            }
        }
    }

    private void PlaceDeadEnd(DoorConnector targetDoor)
    {
        if (deadEndPrefabs.Count == 0 || targetDoor == null || targetDoor.isConnected)
        {
            return;
        }

        GameObject prefabToUse = deadEndPrefabs[Random.Range(0, deadEndPrefabs.Count)];
        GameObject deadEnd = Instantiate(prefabToUse);
        DoorConnector deadEndDoor = deadEnd.GetComponentInChildren<DoorConnector>();

        AlignPiece(targetDoor, deadEndDoor, deadEnd);
        allSpawnedPieces.Add(deadEnd);
        RegisterStandaloneArea(deadEnd, MapAreaShape.Corridor, false);
        targetDoor.isConnected = true;
        deadEndDoor.isConnected = true;
    }

    private void AlignPiece(DoorConnector targetDoor, DoorConnector newDoor, GameObject piece)
    {
        float rotation = (targetDoor.transform.eulerAngles.y + 180f) - newDoor.transform.eulerAngles.y;
        piece.transform.RotateAround(newDoor.transform.position, Vector3.up, rotation);
        piece.transform.position += targetDoor.transform.position - newDoor.transform.position;
        Physics.SyncTransforms();
    }

    private bool HasOverlap(GameObject piece)
    {
        Vector3 checkCenter = piece.transform.position + new Vector3(0f, 2f, 0f);
        Collider[] hitColliders = Physics.OverlapBox(
            checkCenter,
            new Vector3(14.5f, 4f, 14.5f),
            piece.transform.rotation,
            collisionMask
        );

        foreach (Collider hit in hitColliders)
        {
            if (hit.transform.root != piece.transform)
            {
                return true;
            }
        }

        return false;
    }

    private void AddDoorsToList(GameObject piece, bool isCorridor, int owningRoomId, int sourceRoomId)
    {
        foreach (DoorConnector door in piece.GetComponentsInChildren<DoorConnector>())
        {
            if (door.isConnected)
            {
                continue;
            }

            door.owningRoomNodeId = isCorridor ? -1 : owningRoomId;
            door.sourceRoomNodeId = sourceRoomId;

            if (isCorridor)
            {
                pendingCorridorDoors.Add(door);
            }
            else
            {
                pendingRoomDoors.Add(door);
            }
        }
    }

    private DoorConnector TakeRandomPendingDoor(List<DoorConnector> doorList)
    {
        while (doorList.Count > 0)
        {
            int index = Random.Range(0, doorList.Count);
            DoorConnector door = doorList[index];
            doorList.RemoveAt(index);

            if (door != null && !door.isConnected)
            {
                return door;
            }
        }

        return null;
    }

    private int RegisterRoom(GameObject roomObject, DungeonRoomType roomType, bool discoverOnStart)
    {
        Vector2Int gridPosition = WorldToGrid(roomObject.transform.position);
        int roomId = generatedLayout.AddRoom(roomType, gridPosition, roomObject.transform.position);

        RoomInstance roomInstance = roomObject.GetComponent<RoomInstance>();
        if (roomInstance == null)
        {
            roomInstance = roomObject.AddComponent<RoomInstance>();
        }

        roomInstance.Initialize(generatedLayout, roomId, roomType, MapAreaShape.Room, discoverOnStart);
        return roomId;
    }

    private void RegisterStandaloneArea(GameObject areaObject, MapAreaShape areaShape, bool discoverOnStart)
    {
        RoomInstance roomInstance = areaObject.GetComponent<RoomInstance>();
        if (roomInstance == null)
        {
            roomInstance = areaObject.AddComponent<RoomInstance>();
        }

        roomInstance.InitializeStandalone(areaShape, discoverOnStart);
    }

    private Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        if (Mathf.Approximately(mapGridWorldSize, 0f))
        {
            return Vector2Int.zero;
        }

        return new Vector2Int(
            Mathf.RoundToInt(worldPosition.x / mapGridWorldSize),
            Mathf.RoundToInt(worldPosition.z / mapGridWorldSize)
        );
    }
}
