using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class ProceduralTutorial : MonoBehaviour
{
    [Header("Habitaciones del Tutorial")]
    public GameObject salaSpawn;
    public GameObject salaMinimapa;
    public GameObject salaPrePasillo;
    public GameObject pasilloDeLaMuerte;
    public GameObject salaSalida;

    [Header("Habitaciones de Relleno")]
    public List<GameObject> roomPrefabs;

    [Header("Parches")]
    public List<GameObject> deadEndPrefabs;

    [Header("Configuracion")]
    public LayerMask collisionMask;
    public NavMeshSurface navMesh;
    [SerializeField] private DungeonLightingManager lightingManager;

    [Range(0, 3)] public int rellenoAntesDelMinimapa = 1;
    [Range(0, 3)] public int rellenoAntesDelPasillo = 2;

    private readonly List<GameObject> allSpawnedPieces = new List<GameObject>();
    public Vector3 puntoDeGuardadoPasillo { get; private set; }

    public void GenerateTutorialLevel()
    {
        foreach (GameObject piece in allSpawnedPieces)
        {
            if (piece != null) Destroy(piece);
        }
        allSpawnedPieces.Clear();

        GameObject startRoom = Instantiate(salaSpawn, Vector3.zero, Quaternion.identity);
        allSpawnedPieces.Add(startRoom);
        RegisterStandaloneArea(startRoom, MapAreaShape.Room, true);

        DoorConnector currentExit = GetRandomOpenDoor(startRoom);

        currentExit = GenerarTramoDeRelleno(rellenoAntesDelMinimapa, currentExit);

        currentExit = ConectarSalaEspecifica(salaMinimapa, currentExit, out _);

        currentExit = GenerarTramoDeRelleno(rellenoAntesDelPasillo, currentExit);

        GameObject prePasillo;
        currentExit = ConectarSalaEspecifica(salaPrePasillo, currentExit, out prePasillo);
        if (prePasillo != null)
        {
            puntoDeGuardadoPasillo = prePasillo.transform.position;
        }

        currentExit = ConectarSalaEspecifica(pasilloDeLaMuerte, currentExit, out _);

        ConectarSalaEspecifica(salaSalida, currentExit, out _);

        SealOpenDoors();
        ApplyGeneratedLighting();

        if (navMesh != null)
        {
            navMesh.BuildNavMesh();
        }
    }

    private DoorConnector GenerarTramoDeRelleno(int cantidad, DoorConnector currentExit)
    {
        if (roomPrefabs == null || roomPrefabs.Count == 0) return currentExit;

        DoorConnector exitSequence = currentExit;

        for (int i = 0; i < cantidad; i++)
        {
            if (exitSequence == null) break;

            List<GameObject> shuffledRooms = new List<GameObject>(roomPrefabs);
            for (int j = 0; j < shuffledRooms.Count; j++)
            {
                int randomIndex = Random.Range(j, shuffledRooms.Count);
                GameObject temp = shuffledRooms[j];
                shuffledRooms[j] = shuffledRooms[randomIndex];
                shuffledRooms[randomIndex] = temp;
            }

            bool connected = false;
            foreach (GameObject prefab in shuffledRooms)
            {
                DoorConnector newExit = ConectarSalaEspecifica(prefab, exitSequence, out GameObject spawnedRoom);
                if (spawnedRoom != null)
                {
                    exitSequence = newExit;
                    connected = true;
                    break;
                }
            }

            if (!connected) break;
        }

        return exitSequence;
    }

    private DoorConnector ConectarSalaEspecifica(GameObject prefab, DoorConnector targetDoor, out GameObject spawnedPiece)
    {
        spawnedPiece = null;

        if (targetDoor == null)
        {
            Debug.LogWarning($"[Tutorial] Falla en {prefab?.name}: La sala anterior no dejo ninguna puerta de salida libre.");
            return null;
        }

        if (prefab == null) return targetDoor;

        GameObject newPiece = Instantiate(prefab);
        DoorConnector[] newDoors = newPiece.GetComponentsInChildren<DoorConnector>();
        bool aligned = false;

        foreach (DoorConnector newDoor in newDoors)
        {
            if (newDoor.doorHeightOffset != targetDoor.doorHeightOffset) continue;

            AlignPiece(targetDoor, newDoor, newPiece);

            if (!HasOverlap(newPiece))
            {
                targetDoor.isConnected = true;
                newDoor.isConnected = true;
                allSpawnedPieces.Add(newPiece);
                RegisterStandaloneArea(newPiece, MapAreaShape.Room, false);

                spawnedPiece = newPiece;
                return GetRandomOpenDoor(newPiece);
            }
            aligned = true;
        }

        if (aligned)
        {
            Debug.LogWarning($"[Tutorial] Falla en {prefab.name}: El BoxCollider choca con otra habitacion (Overlap).");
        }
        else
        {
            Debug.LogWarning($"[Tutorial] Falla en {prefab.name}: No tiene puertas compatibles o no tiene el componente DoorConnector.");
        }

        Destroy(newPiece);
        return targetDoor;
    }

    private DoorConnector GetRandomOpenDoor(GameObject piece)
    {
        List<DoorConnector> openDoors = new List<DoorConnector>();
        foreach (DoorConnector door in piece.GetComponentsInChildren<DoorConnector>())
        {
            if (!door.isConnected) openDoors.Add(door);
        }

        if (openDoors.Count > 0) return openDoors[Random.Range(0, openDoors.Count)];
        return null;
    }

    private void SealOpenDoors()
    {
        List<GameObject> piecesToCheck = new List<GameObject>(allSpawnedPieces);

        foreach (GameObject piece in piecesToCheck)
        {
            if (piece == null) continue;

            foreach (DoorConnector door in piece.GetComponentsInChildren<DoorConnector>())
            {
                if (!door.isConnected)
                {
                    PlaceDeadEnd(door);
                }
            }
        }
    }

    private void PlaceDeadEnd(DoorConnector targetDoor)
    {
        if (deadEndPrefabs.Count == 0 || targetDoor == null || targetDoor.isConnected) return;

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
        BoxCollider roomCollider = piece.GetComponent<BoxCollider>();

        if (roomCollider != null)
        {
            Vector3 dynamicSize = roomCollider.size / 2f * 0.95f;
            Vector3 checkCenter = piece.transform.TransformPoint(roomCollider.center);

            Collider[] hitColliders = Physics.OverlapBox(
                checkCenter,
                dynamicSize,
                piece.transform.rotation,
                collisionMask
            );

            foreach (Collider hit in hitColliders)
            {
                if (hit.transform.root != piece.transform.root)
                {
                    return true;
                }
            }
            return false;
        }
        return true;
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

    private void ApplyGeneratedLighting()
    {
        if (lightingManager == null)
        {
            lightingManager = FindFirstObjectByType<DungeonLightingManager>();
        }

        if (lightingManager != null)
        {
            lightingManager.ApplyLighting(allSpawnedPieces);
        }
    }
}