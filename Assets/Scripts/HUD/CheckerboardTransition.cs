using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CheckerboardTransition : MonoBehaviour
{
    public static CheckerboardTransition Instance { get; private set; }
    public static bool directToMenu = false;

    [SerializeField] private List<Image> squares = new List<Image>();
    [SerializeField] private GameObject squarePrefab;
    [SerializeField] private float squareSize = 50f;
    [SerializeField] private float animationDuration = 0.3f; // Más rápido
    [SerializeField] private float delayBetweenColumns = 0.02f; // Delay entre columnas, más rápido
    [SerializeField] private float speed = 2f; // Multiplicador de velocidad, más rápido

    private int columns;
    private int rows;

    public bool IsTransitioning { get; private set; } = false;
    private Action onComplete;
    private Image raycastBlocker;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) { canvas = gameObject.AddComponent<Canvas>(); }
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null) { scaler = gameObject.AddComponent<CanvasScaler>(); }
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        if (GetComponent<GraphicRaycaster>() == null) { gameObject.AddComponent<GraphicRaycaster>(); }


        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        raycastBlocker = GetComponent<Image>();
        if (raycastBlocker == null) { raycastBlocker = gameObject.AddComponent<Image>(); }
        raycastBlocker.color = Color.clear;
        raycastBlocker.raycastTarget = false;
        IsTransitioning = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsTransitioning)
        {
            IsTransitioning = true;
        }

        if (raycastBlocker != null) raycastBlocker.raycastTarget = true;
        UpdateGrid(true);
        if (IsLevelScene(scene.name))
        {
            StartCoroutine(FadeOutAfterFloorSign());
        }
        else
        {
            StartCoroutine(AnimateFadeOut());
        }
    }

    private void Start()
    {
        if (squarePrefab != null)
        {
            UpdateGrid();
        }
    }

    private void UpdateGrid(bool startBlack = false)
    {
        Canvas.ForceUpdateCanvases();

        int oldColumns = columns;
        int oldRows = rows;
        CalculateGridSize();

        if (squares.Count == 0 || oldColumns != columns || oldRows != rows)
        {
            foreach (Transform child in transform) { Destroy(child.gameObject); }
            squares.Clear();
            CreateSquares(startBlack);
        }
        else if (startBlack)
        {
            foreach (var square in squares)
            {
                square.color = Color.black;
            }
        }
    }

    private void CalculateGridSize()
    {
        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            columns = Mathf.CeilToInt(rect.rect.width / squareSize);
            rows = Mathf.CeilToInt(rect.rect.height / squareSize);

            var grid = GetComponent<UnityEngine.UI.GridLayoutGroup>();
            if (grid != null)
            {
                grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = columns;
                grid.cellSize = new Vector2(squareSize, squareSize);
                grid.spacing = Vector2.zero;
            }
        }
        else
        {
            columns = 20;
            rows = 20;
        }
    }

    private void CreateSquares(bool startBlack = false)
    {
        int totalSquares = columns * rows;
        for (int i = 0; i < totalSquares; i++)
        {
            GameObject go = Instantiate(squarePrefab, transform);
            go.name = "Square" + i;
            Image img = go.GetComponent<Image>();
            if (img != null)
            {
                img.color = startBlack ? Color.black : Color.clear;
                img.raycastTarget = false;
                squares.Add(img);
            }
        }
        Debug.Log($"{totalSquares} squares created from prefab! Grid: {columns}x{rows}");

        for (int i = 0; i < Mathf.Min(squares.Count, 10); i++)
        {
            int x = i % columns;
            int y = i / columns;
            bool isVisible = (x + y) % 2 == 0;
            Debug.Log($"Square {i}: x={x}, y={y}, visible={isVisible}");
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            Gizmos.color = Color.red;
            Vector3 center = rect.TransformPoint(rect.rect.center);
            Vector3 size = new Vector3(rect.rect.width, rect.rect.height, 0f);
            Gizmos.DrawWireCube(center, size);
            Handles.Label(center, $"Transition Area ({rect.rect.width}x{rect.rect.height})");
        }
    }
#endif

    public void StartTransition(Action onCompleteCallback, bool isSceneLoad = true)
    {
        if (IsTransitioning)
        {
            Debug.LogWarning("Ya hay una transición en curso. Se ignora la nueva solicitud.");
            return;
        }
        IsTransitioning = true;
        if (raycastBlocker != null) raycastBlocker.raycastTarget = true;
        UpdateGrid();
        onComplete = onCompleteCallback;
        StartCoroutine(AnimateTransitionFlow(isSceneLoad));
    }

    public static void LoadScene(string sceneName)
    {
        if (Instance != null)
        {
            Instance.StartTransition(() => SceneManager.LoadScene(sceneName), true);
        }
        else
        {
            Debug.LogWarning("CheckerboardTransition.Instance not found in current scene. Loading scene directly.");
            SceneManager.LoadScene(sceneName);
        }
    }

    private IEnumerator AnimateTransitionFlow(bool isSceneLoad)
    {
        List<List<Image>> visibleColumns = new List<List<Image>>();
        List<List<Image>> hiddenColumns = new List<List<Image>>();

        for (int x = 0; x < columns; x++)
        {
            visibleColumns.Add(new List<Image>());
            hiddenColumns.Add(new List<Image>());
        }

        for (int i = 0; i < squares.Count; i++)
        {
            int x = i % columns;
            int y = i / columns;
            bool isVisible = (x + y) % 2 == 0;

            if (isVisible)
            {
                visibleColumns[x].Add(squares[i]);
            }
            else
            {
                hiddenColumns[x].Add(squares[i]);
            }
        }

        float adjustedDuration = animationDuration / speed;

        for (int x = 0; x < columns; x++)
        {
            foreach (Image square in visibleColumns[x])
            {
                StartCoroutine(FadeIn(square, adjustedDuration));
            }

            yield return new WaitForSecondsRealtime(delayBetweenColumns);
        }

        yield return new WaitForSecondsRealtime(adjustedDuration);

        for (int x = 0; x < columns; x++)
        {
            foreach (Image square in hiddenColumns[x])
            {
                StartCoroutine(FadeIn(square, adjustedDuration));
            }

            yield return new WaitForSecondsRealtime(delayBetweenColumns);
        }

        yield return new WaitForSecondsRealtime(adjustedDuration);

        if (isSceneLoad)
        {
            onComplete?.Invoke();
        }
        else
        {
            onComplete?.Invoke();
            yield return StartCoroutine(AnimateFadeOut());
        }
    }

    private IEnumerator FadeOutAfterFloorSign()
    {
        // Espera hasta que el cartel del piso haya desaparecido
        yield return new WaitUntil(() => !PauseScreen.IsFloorSignActive);
        yield return StartCoroutine(AnimateFadeOut());
    }

    private bool IsLevelScene(string sceneName)
    {
        return sceneName.StartsWith("Level_");
    }

    private IEnumerator AnimateFadeOut()
    {
        float adjustedDuration = animationDuration / speed;

        List<List<Image>> visibleColumns = new List<List<Image>>();
        List<List<Image>> hiddenColumns = new List<List<Image>>();

        for (int x = 0; x < columns; x++)
        {
            visibleColumns.Add(new List<Image>());
            hiddenColumns.Add(new List<Image>());
        }

        for (int i = 0; i < squares.Count; i++)
        {
            int x = i % columns;
            int y = i / columns;
            bool isVisiblePattern = (x + y) % 2 == 0;

            if (isVisiblePattern)
            {
                visibleColumns[x].Add(squares[i]);
            }
            else
            {
                hiddenColumns[x].Add(squares[i]);
            }
        }

        for (int x = columns - 1; x >= 0; x--)
        {
            foreach (Image square in hiddenColumns[x])
            {
                StartCoroutine(FadeOut(square, adjustedDuration));
            }
            yield return new WaitForSecondsRealtime(delayBetweenColumns);
        }

        yield return new WaitForSecondsRealtime(adjustedDuration);

        for (int x = columns - 1; x >= 0; x--)
        {
            foreach (Image square in visibleColumns[x])
            {
                StartCoroutine(FadeOut(square, adjustedDuration));
            }
            yield return new WaitForSecondsRealtime(delayBetweenColumns);
        }

        yield return new WaitForSecondsRealtime(adjustedDuration);

        foreach (Image square in squares) { square.color = Color.clear; }
        IsTransitioning = false;
        if (raycastBlocker != null) raycastBlocker.raycastTarget = false;
    }

    private IEnumerator FadeIn(Image image, float duration)
    {
        float elapsed = 0f;
        Color startColor = image.color;
        Color endColor = new Color(0, 0, 0, 1);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            image.color = Color.Lerp(startColor, endColor, elapsed / duration);
            yield return null;
        }

        image.color = endColor;
    }

    private IEnumerator FadeOut(Image image, float duration)
    {
        float elapsed = 0f;
        Color startColor = image.color;
        Color endColor = new Color(0, 0, 0, 0);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            image.color = Color.Lerp(startColor, endColor, elapsed / duration);
            yield return null;
        }

        image.color = endColor;
    }

}