using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomEventDirector : MonoBehaviour
{
    public static RoomEventDirector Instance { get; private set; }

    [Header("Floor Rules")]
    [SerializeField] private int firstEventFloor = 2;
    [SerializeField] private int earlyFloorMax = 3;
    [SerializeField] private int earlyFloorEventLimit = 1;
    [SerializeField] private int advancedFloorEventLimit = 2;
    [SerializeField] private int minimumNormalRoomsBetweenEvents = 2;

    [Header("Probability")]
    [SerializeField, Range(0f, 1f)] private float baseChance = 0.15f;
    [SerializeField, Range(0f, 1f)] private float pityIncrement = 0.10f;

    [Header("Debug")]
    [SerializeField] private bool showDebugButtons = false;
    [SerializeField] private KeyCode debugStartEventKey = KeyCode.I;
    [SerializeField] private KeyCode debugClearWavesKey = KeyCode.L;

    private int trackedFloor = -1;
    private int eventsThisFloor;
    private int roomsSinceLastEvent;
    private float currentChance;
    private bool isFallbackInstance;
    private Canvas debugCanvas;
    private readonly List<RoomEventType> usedEventsThisFloor = new List<RoomEventType>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject directorObject = new GameObject("Room Event Director");
        RoomEventDirector director = directorObject.AddComponent<RoomEventDirector>();
        director.isFallbackInstance = true;
        DontDestroyOnLoad(directorObject);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (Instance.isFallbackInstance && !isFallbackInstance)
            {
                Destroy(Instance.gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        if (!isFallbackInstance)
        {
            DontDestroyOnLoad(gameObject);
        }

        Instance = this;

        if (isFallbackInstance)
        {
            DontDestroyOnLoad(gameObject);
        }

        ResetForFloor(LevelManager.currentLevel);
        BuildDebugButtons();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnValidate()
    {
        earlyFloorEventLimit = Mathf.Max(0, earlyFloorEventLimit);
        advancedFloorEventLimit = Mathf.Max(0, advancedFloorEventLimit);
        minimumNormalRoomsBetweenEvents = Mathf.Max(0, minimumNormalRoomsBetweenEvents);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Update()
    {
        if (Input.GetKeyDown(debugStartEventKey))
        {
            DebugStartUnexpectedEventInCurrentRoom();
        }

        if (Input.GetKeyDown(debugClearWavesKey))
        {
            DebugClearCurrentRoomWaves();
        }
    }

    public bool TryStartUnexpectedEvent(RoomInstance room)
    {
        if (!CanRollForRoom(room))
        {
            return false;
        }

        EnsureFloorState();
        roomsSinceLastEvent++;

        if (eventsThisFloor > 0 && roomsSinceLastEvent <= minimumNormalRoomsBetweenEvents)
        {
            return false;
        }

        if (UnityEngine.Random.value > currentChance)
        {
            currentChance = Mathf.Clamp01(currentChance + pityIncrement);
            return false;
        }

        if (!TryPickUnusedEvent(out RoomEventType selectedEvent))
        {
            return false;
        }

        eventsThisFloor++;
        roomsSinceLastEvent = 0;
        currentChance = baseChance;

        RoomEventRunner runner = room.GetComponent<RoomEventRunner>();
        if (runner == null)
        {
            runner = room.gameObject.AddComponent<RoomEventRunner>();
        }

        usedEventsThisFloor.Add(selectedEvent);
        runner.StartEvent(selectedEvent, room);
        return true;
    }

    public void DebugStartUnexpectedEventInCurrentRoom()
    {
        RoomInstance room = GetCurrentRoom();
        if (room == null)
        {
            Debug.LogWarning("Debug evento inesperado: no hay habitacion actual.");
            return;
        }

        RoomEventRunner runner = room.GetComponent<RoomEventRunner>();
        if (runner == null)
        {
            runner = room.gameObject.AddComponent<RoomEventRunner>();
        }

        RoomEventType selectedEvent = (RoomEventType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(RoomEventType)).Length);
        runner.StartEvent(selectedEvent, room);
        Debug.Log($"Debug evento inesperado invocado en {room.name}: {selectedEvent}");
    }

    public void DebugClearCurrentRoomWaves()
    {
        RoomInstance room = GetCurrentRoom();
        if (room == null)
        {
            Debug.LogWarning("Debug limpiar oleadas: no hay habitacion actual.");
            return;
        }

        RoomEventRunner runner = room.GetComponent<RoomEventRunner>();
        if (runner != null)
        {
            runner.DebugClearEvent();
        }

        RoomEnemySpawner spawner = room.GetComponent<RoomEnemySpawner>();
        if (spawner != null)
        {
            spawner.DebugClearWaves();
        }

        DebugDestroyEnemiesInsideRoom(room);
        Debug.Log($"Debug oleadas limpiadas en {room.name}");
    }

    private bool CanRollForRoom(RoomInstance room)
    {
        if (room == null)
        {
            return false;
        }

        EnsureFloorState();

        if (LevelManager.currentLevel < firstEventFloor)
        {
            return false;
        }

        if (SceneManager.GetActiveScene().name == "Tutorial")
        {
            return false;
        }

        if (room.RoomType != DungeonRoomType.Normal || room.AreaShape != MapAreaShape.Room)
        {
            return false;
        }

        return eventsThisFloor < GetEventLimitForCurrentFloor();
    }

    private void EnsureFloorState()
    {
        if (trackedFloor != LevelManager.currentLevel)
        {
            ResetForFloor(LevelManager.currentLevel);
        }
    }

    private int GetEventLimitForCurrentFloor()
    {
        return LevelManager.currentLevel <= earlyFloorMax ? earlyFloorEventLimit : advancedFloorEventLimit;
    }

    private RoomInstance GetCurrentRoom()
    {
        IReadOnlyList<RoomInstance> rooms = RoomInstance.ActiveInstances;
        for (int i = 0; i < rooms.Count; i++)
        {
            RoomInstance room = rooms[i];
            if (room != null && room.IsCurrentArea)
            {
                return room;
            }
        }

        return null;
    }

    private void DebugDestroyEnemiesInsideRoom(RoomInstance room)
    {
        if (room == null || !room.HasBounds)
        {
            return;
        }

        Bounds roomBounds = room.WorldBounds;
        roomBounds.Expand(new Vector3(2f, 6f, 2f));

        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy != null && roomBounds.Contains(enemy.transform.position))
            {
                Destroy(enemy.gameObject);
            }
        }
    }

    private bool TryPickUnusedEvent(out RoomEventType selectedEvent)
    {
        Array eventValues = Enum.GetValues(typeof(RoomEventType));
        List<RoomEventType> availableEvents = new List<RoomEventType>();

        for (int i = 0; i < eventValues.Length; i++)
        {
            RoomEventType eventType = (RoomEventType)eventValues.GetValue(i);
            if (!usedEventsThisFloor.Contains(eventType))
            {
                availableEvents.Add(eventType);
            }
        }

        if (availableEvents.Count == 0)
        {
            selectedEvent = default;
            return false;
        }

        selectedEvent = availableEvents[UnityEngine.Random.Range(0, availableEvents.Count)];
        return true;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetForFloor(LevelManager.currentLevel);
    }

    private void ResetForFloor(int floor)
    {
        trackedFloor = floor;
        eventsThisFloor = 0;
        roomsSinceLastEvent = 0;
        currentChance = baseChance;
        usedEventsThisFloor.Clear();
    }

    private void BuildDebugButtons()
    {
        if (!showDebugButtons || debugCanvas != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("Room Event Debug UI");
        canvasObject.transform.SetParent(transform, false);

        debugCanvas = canvasObject.AddComponent<Canvas>();
        debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        debugCanvas.sortingOrder = 950;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject panelObject = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(canvasObject.transform, false);

        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(0f, 0f);
        panel.anchorMax = new Vector2(0f, 0f);
        panel.pivot = new Vector2(0f, 0f);
        panel.anchoredPosition = new Vector2(16f, 16f);
        panel.sizeDelta = new Vector2(260f, 96f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.02f, 0.018f, 0.02f, 0.72f);
        panelImage.raycastTarget = false;

        CreateDebugButton(panel, $"[{debugStartEventKey}] Invocar evento", new Vector2(8f, -8f), DebugStartUnexpectedEventInCurrentRoom);
        CreateDebugButton(panel, $"[{debugClearWavesKey}] Limpiar oleadas", new Vector2(8f, -52f), DebugClearCurrentRoomWaves);
    }

    private void CreateDebugButton(RectTransform parent, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(244f, 36f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.16f, 0.03f, 0.04f, 0.92f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 18f;
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.raycastTarget = false;
    }
}
