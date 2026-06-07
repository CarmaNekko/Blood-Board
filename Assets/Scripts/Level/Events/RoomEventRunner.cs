using System.Collections;
using BloodBoard.GameManagement;
using UnityEngine;

public enum RoomEventType
{
    CrimsonAmbush,
    PolarityDuel,
    BloodClock
}

public class RoomEventRunner : MonoBehaviour
{
    private const float DoorSafetyDistance = 7f;

    private bool isRunning;
    private RoomInstance eventRoom;
    private RoomEnemySpawner roomSpawner;
    private readonly System.Collections.Generic.List<GameObject> activeEventEnemies = new System.Collections.Generic.List<GameObject>();

    public void StartRandomEvent(RoomInstance room)
    {
        StartEvent((RoomEventType)Random.Range(0, System.Enum.GetValues(typeof(RoomEventType)).Length), room);
    }

    public void StartEvent(RoomEventType selectedEvent, RoomInstance room = null)
    {
        if (isRunning)
        {
            return;
        }

        eventRoom = room != null ? room : GetComponent<RoomInstance>();
        StartCoroutine(RunEvent(selectedEvent));
    }

    private IEnumerator RunEvent(RoomEventType selectedEvent)
    {
        isRunning = true;

        switch (selectedEvent)
        {
            case RoomEventType.CrimsonAmbush:
                yield return RunCrimsonAmbush();
                break;
            case RoomEventType.PolarityDuel:
                yield return RunPolarityDuel();
                break;
            default:
                yield return RunBloodClock();
                break;
        }

        isRunning = false;
    }

    private IEnumerator RunCrimsonAmbush()
    {
        EventAnnouncementUI.ShowMessage(
            "EVENTO: EMBOSCADA CARMESI",
            "Enemigos fortalecidos sellan la sala.",
            3f);

        yield return WaitForPlayerReadyToLockDoors();
        LockDoors(true);

        yield return SpawnEventEnemies(Mathf.Clamp(LevelManager.currentLevel + 1, 3, 6), true);
        yield return WaitForEventEnemiesCleared();

        MagicShooter shooter = FindFirstObjectByType<MagicShooter>();
        if (shooter != null)
        {
            shooter.RefillManaToMax();
        }

        ScoreManager.Instance?.AddScoreToCurrent(125);
        LockDoors(false);

        EventAnnouncementUI.ShowMessage(
            "EMBOSCADA SUPERADA",
            "Mana restaurado y puntuacion extra por sobrevivir al asalto.",
            2.5f);

        yield return new WaitForSeconds(2.5f);
    }

    private IEnumerator RunPolarityDuel()
    {
        MagicColor chosenColor = Random.value < 0.5f ? MagicColor.White : MagicColor.Black;
        string colorName = chosenColor == MagicColor.White ? "BLANCA" : "NEGRA";

        EventAnnouncementUI.ShowMessage(
            $"EVENTO: DUELO DE POLARIDAD {colorName}",
            "Solo una polaridad domina esta sala.",
            3f);

        yield return WaitForPlayerReadyToLockDoors();
        LockDoors(true);

        yield return SpawnEventEnemies(Mathf.Clamp(LevelManager.currentLevel + 2, 4, 7), false, chosenColor);
        yield return WaitForEventEnemiesCleared();

        MagicShooter shooter = FindFirstObjectByType<MagicShooter>();
        if (shooter != null)
        {
            shooter.RefillManaToMax();
        }

        ScoreManager.Instance?.AddScoreToCurrent(100);
        LockDoors(false);

        EventAnnouncementUI.ShowMessage(
            "DUELO SUPERADO",
            "Mana restaurado y puntuacion extra por estabilizar la sala.",
            2.5f);

        yield return new WaitForSeconds(2.5f);
    }

    private IEnumerator RunBloodClock()
    {
        const float eventDuration = 25f;

        EventAnnouncementUI.ShowMessage(
            "EVENTO: RELOJ DE SANGRE",
            "Elimina la oleada antes de que el sello hiera al jugador.",
            2.5f);

        yield return WaitForPlayerReadyToLockDoors();
        LockDoors(true);
        yield return SpawnEventEnemies(Mathf.Clamp(LevelManager.currentLevel + 2, 4, 8), false);

        Coroutine countdown = StartCoroutine(EventAnnouncementUI.ShowCountdown(
            "EVENTO: RELOJ DE SANGRE",
            "Limpia la sala antes de que termine el contador.",
            eventDuration));

        float deadline = Time.time + eventDuration;
        bool clearedInTime = false;
        bool timedOut = false;

        while (true)
        {
            ClearDeadEventEnemies();
            if (activeEventEnemies.Count == 0)
            {
                clearedInTime = Time.time < deadline;
                timedOut = !clearedInTime;
                break;
            }

            if (Time.time >= deadline)
            {
                timedOut = true;
                break;
            }

            yield return null;
        }

        if (countdown != null)
        {
            StopCoroutine(countdown);
        }

        if (timedOut)
        {
            PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(15f);
            }

            EventAnnouncementUI.ShowMessage(
                "EL RELOJ COBRA SANGRE",
                "Acaba con los enemigos restantes.",
                2.5f);

            yield return WaitForEventEnemiesCleared();
        }

        if (clearedInTime)
        {
            MagicShooter shooter = FindFirstObjectByType<MagicShooter>();
            if (shooter != null)
            {
                shooter.RefillManaToMax();
            }

            ScoreManager.Instance?.AddScoreToCurrent(175);
        }

        LockDoors(false);

        EventAnnouncementUI.ShowMessage(
            clearedInTime ? "RELOJ SUPERADO" : "TIEMPO AGOTADO",
            clearedInTime ? "Puntuacion extra por limpiar a tiempo." : "Sala limpiada sin bonificacion.",
            2.5f);

        yield return new WaitForSeconds(2.5f);
    }

    private void LockDoors(bool lockState)
    {
        if (roomSpawner == null)
        {
            roomSpawner = GetComponent<RoomEnemySpawner>();
        }

        if (roomSpawner != null)
        {
            roomSpawner.SetDoorsLockedForExternalEvent(lockState);
            return;
        }

        foreach (DoorConnector door in GetComponentsInChildren<DoorConnector>())
        {
            if (door.isConnected)
            {
                door.SetLock(lockState);
            }
        }
    }

    private IEnumerator SpawnEventEnemies(int count, bool buffEnemies, MagicColor? requiredColor = null)
    {
        activeEventEnemies.Clear();

        if (roomSpawner == null)
        {
            roomSpawner = GetComponent<RoomEnemySpawner>();
        }

        if (roomSpawner == null || roomSpawner.SpawnPoints == null || roomSpawner.SpawnPoints.Count == 0)
        {
            yield break;
        }

        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        System.Collections.Generic.List<GameObject> pool = levelManager != null
            ? levelManager.GetAllowedEnemies()
            : new System.Collections.Generic.List<GameObject>();

        if (requiredColor.HasValue)
        {
            pool.RemoveAll(enemyPrefab =>
            {
                EnemyHealth health = enemyPrefab != null ? enemyPrefab.GetComponent<EnemyHealth>() : null;
                return health == null || health.myColor != requiredColor.Value;
            });
        }

        if (pool.Count == 0)
        {
            yield break;
        }

        yield return roomSpawner.SpawnExternalEnemiesRoutine(count, pool, buffEnemies, activeEventEnemies);
    }

    private IEnumerator WaitForPlayerReadyToLockDoors()
    {
        Transform playerTransform = ResolvePlayerTransform();
        DoorConnector[] doors = GetComponentsInChildren<DoorConnector>();

        if (playerTransform == null || doors == null || doors.Length == 0)
        {
            yield return new WaitForSeconds(0.75f);
            yield break;
        }

        while (!IsPlayerReadyForDoorLock(playerTransform, doors))
        {
            yield return null;
        }
    }

    private Transform ResolvePlayerTransform()
    {
        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null)
        {
            return taggedPlayer.transform;
        }

        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        return playerMovement != null ? playerMovement.transform : null;
    }

    private bool IsPlayerReadyForDoorLock(Transform playerTransform, DoorConnector[] doors)
    {
        if (eventRoom != null && !eventRoom.IsCurrentArea)
        {
            return false;
        }

        Vector2 playerPos2D = new Vector2(playerTransform.position.x, playerTransform.position.z);
        foreach (DoorConnector door in doors)
        {
            if (door == null || !door.isConnected)
            {
                continue;
            }

            Vector2 doorPos2D = new Vector2(door.transform.position.x, door.transform.position.z);
            if (Vector2.Distance(playerPos2D, doorPos2D) < DoorSafetyDistance)
            {
                return false;
            }
        }

        return true;
    }

    private IEnumerator WaitForEventEnemiesCleared()
    {
        while (true)
        {
            ClearDeadEventEnemies();
            if (activeEventEnemies.Count == 0)
            {
                yield break;
            }

            yield return null;
        }
    }

    private void ClearDeadEventEnemies()
    {
        activeEventEnemies.RemoveAll(enemy => enemy == null);
    }

    public void DebugClearEvent()
    {
        StopAllCoroutines();

        foreach (GameObject enemy in activeEventEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }

        activeEventEnemies.Clear();
        isRunning = false;
        LockDoors(false);
    }
}
