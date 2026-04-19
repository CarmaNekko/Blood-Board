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

    [Header("Configuracioncioncion")]
    public int maxRooms = 6;
    public LayerMask collisionMask;
    public NavMeshSurface navMesh;

    private List<DoorConnector> pendingRoomDoors = new List<DoorConnector>();
    private List<DoorConnector> pendingCorridorDoors = new List<DoorConnector>();
    private List<GameObject> allSpawnedPieces = new List<GameObject>();
    private int roomCount = 0;

    void Start() => GenerateLevel();

    void GenerateLevel()
    {
        pendingRoomDoors.Clear();
        pendingCorridorDoors.Clear();
        allSpawnedPieces.Clear();
        roomCount = 0;

        GameObject startRoom = Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        allSpawnedPieces.Add(startRoom);
        roomCount++;
        AddDoorsToList(startRoom, false);

        int attempts = 0;
        while (roomCount < maxRooms && (pendingRoomDoors.Count > 0 || pendingCorridorDoors.Count > 0) && attempts < 2000)
        {
            attempts++;
            SpawnNextPiece();
        }

        PlaceFinalRoom();
        SealOpenDoors();

        if (navMesh != null) navMesh.BuildNavMesh();

        Debug.Log($"Calabozo generado. Habitaciones: {roomCount}. Intentos: {attempts}");
    }

    void SpawnNextPiece()
    {
        if (pendingCorridorDoors.Count > 0)
        {
            int index = Random.Range(0, pendingCorridorDoors.Count);
            DoorConnector targetDoor = pendingCorridorDoors[index];
            pendingCorridorDoors.RemoveAt(index);
            TryConnect(targetDoor, roomPrefabs, false);
        }
        else if (pendingRoomDoors.Count > 0)
        {
            int index = Random.Range(0, pendingRoomDoors.Count);
            DoorConnector targetDoor = pendingRoomDoors[index];
            pendingRoomDoors.RemoveAt(index);
            TryConnect(targetDoor, corridorPrefabs, true);
        }
    }

    bool TryConnect(DoorConnector targetDoor, List<GameObject> prefabList, bool isCorridor)
    {
        List<GameObject> shuffled = new List<GameObject>(prefabList);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int r = Random.Range(i, shuffled.Count);
            var tmp = shuffled[i]; shuffled[i] = shuffled[r]; shuffled[r] = tmp;
        }

        foreach (GameObject prefab in shuffled)
        {
            GameObject newPiece = Instantiate(prefab);
            DoorConnector[] newDoors = newPiece.GetComponentsInChildren<DoorConnector>();

            foreach (DoorConnector newDoor in newDoors)
            {
                if (newDoor.doorHeightOffset == targetDoor.doorHeightOffset)
                {
                    AlignPiece(targetDoor, newDoor, newPiece);

                    if (!HasOverlap(newPiece))
                    {
                        allSpawnedPieces.Add(newPiece);
                        if (!isCorridor) roomCount++;

                        AddDoorsToList(newPiece, isCorridor);
                        newDoor.isConnected = true;
                        targetDoor.isConnected = true;
                        return true;
                    }
                }
            }
            newPiece.SetActive(false);
            Destroy(newPiece);
        }
        return false;
    }

    void PlaceFinalRoom()
    {
        List<DoorConnector> candidates = new List<DoorConnector>(pendingCorridorDoors);
        candidates.AddRange(pendingRoomDoors);

        for (int i = candidates.Count - 1; i >= 0; i--)
        {
            DoorConnector targetDoor = candidates[i];
            if (targetDoor.isConnected) continue;

            GameObject final = Instantiate(finalRoomPrefab);
            DoorConnector fDoor = final.GetComponentInChildren<DoorConnector>();

            if (fDoor.doorHeightOffset == targetDoor.doorHeightOffset)
            {
                AlignPiece(targetDoor, fDoor, final);

                if (!HasOverlap(final))
                {
                    targetDoor.isConnected = true;
                    fDoor.isConnected = true;
                    return;
                }
            }
            final.SetActive(false);
            Destroy(final);
        }
    }

    void SealOpenDoors()
    {
        foreach (DoorConnector cDoor in pendingCorridorDoors)
        {
            if (!cDoor.isConnected) PlaceDeadEnd(cDoor);
        }

        foreach (DoorConnector rDoor in pendingRoomDoors)
        {
            if (rDoor.isConnected) continue;

            bool corridorPlaced = false;
            foreach (GameObject corridorPrefab in corridorPrefabs)
            {
                GameObject corridor = Instantiate(corridorPrefab);
                DoorConnector cEntrance = corridor.GetComponentInChildren<DoorConnector>();

                if (cEntrance.doorHeightOffset == rDoor.doorHeightOffset)
                {
                    AlignPiece(rDoor, cEntrance, corridor);

                    if (!HasOverlap(corridor))
                    {
                        rDoor.isConnected = true;
                        cEntrance.isConnected = true;
                        corridorPlaced = true;

                        foreach (DoorConnector cExit in corridor.GetComponentsInChildren<DoorConnector>())
                        {
                            if (!cExit.isConnected) PlaceDeadEnd(cExit);
                        }
                        break;
                    }
                }
                corridor.SetActive(false);
                Destroy(corridor);
            }

            if (!corridorPlaced) PlaceDeadEnd(rDoor);
        }
    }

    void PlaceDeadEnd(DoorConnector targetDoor)
    {
        if (deadEndPrefabs.Count == 0) return;

        GameObject prefabToUse = deadEndPrefabs[Random.Range(0, deadEndPrefabs.Count)];
        GameObject deadEnd = Instantiate(prefabToUse);
        DoorConnector dDoor = deadEnd.GetComponentInChildren<DoorConnector>();

        AlignPiece(targetDoor, dDoor, deadEnd);
        targetDoor.isConnected = true;
        dDoor.isConnected = true;
    }

    void AlignPiece(DoorConnector targetDoor, DoorConnector newDoor, GameObject piece)
    {
        float rot = (targetDoor.transform.eulerAngles.y + 180f) - newDoor.transform.eulerAngles.y;
        piece.transform.RotateAround(newDoor.transform.position, Vector3.up, rot);
        piece.transform.position += (targetDoor.transform.position - newDoor.transform.position);
        Physics.SyncTransforms();
    }

    bool HasOverlap(GameObject piece)
    {
        Vector3 checkCenter = piece.transform.position + new Vector3(0, 2f, 0);
        Collider[] hitColliders = Physics.OverlapBox(checkCenter, new Vector3(14.5f, 4f, 14.5f), piece.transform.rotation, collisionMask);

        foreach (var hit in hitColliders)
        {
            if (hit.transform.root != piece.transform) return true;
        }
        return false;
    }

    void AddDoorsToList(GameObject piece, bool isCorridor)
    {
        foreach (DoorConnector d in piece.GetComponentsInChildren<DoorConnector>())
        {
            if (!d.isConnected)
            {
                if (isCorridor) pendingCorridorDoors.Add(d);
                else pendingRoomDoors.Add(d);
            }
        }
    }
}