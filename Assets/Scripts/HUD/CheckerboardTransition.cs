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

    [SerializeField] private List<Image> squares = new List<Image>();
    [SerializeField] private GameObject squarePrefab;
    [SerializeField] private float squareSize = 50f;
    [SerializeField] private float animationDuration = 0.3f; // Más rápido
    [SerializeField] private float delayBetweenColumns = 0.02f; // Delay entre columnas, más rápido
    [SerializeField] private float speed = 2f; // Multiplicador de velocidad, más rápido

    private int columns;
    private int rows;

    private Action onComplete;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Asegurar que el panel cubra toda la pantalla
        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        if (squares.Count == 0 && squarePrefab != null)
        {
            CalculateGridSize();
            CreateSquares();
        }
    }

    private void CalculateGridSize()
    {
        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            columns = Mathf.CeilToInt(rect.rect.width / squareSize);
            rows = Mathf.CeilToInt(rect.rect.height / squareSize);

            // Configurar GridLayoutGroup
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

    private void CreateSquares()
    {
        int totalSquares = columns * rows;
        for (int i = 0; i < totalSquares; i++)
        {
            GameObject go = Instantiate(squarePrefab, transform);
            go.name = "Square" + i;
            Image img = go.GetComponent<Image>();
            if (img != null)
            {
                img.color = Color.clear; // Iniciar transparentes
                squares.Add(img);
            }
        }
        Debug.Log($"{totalSquares} squares created from prefab! Grid: {columns}x{rows}");

        // Verificar patrón ajedrez
        for (int i = 0; i < Mathf.Min(squares.Count, 10); i++) // Primeros 10
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

    public void StartTransition(Action onCompleteCallback)
    {
        onComplete = onCompleteCallback;
        StartCoroutine(AnimateCheckerboard());
    }

    public static void LoadScene(string sceneName)
    {
        if (Instance != null)
        {
            Instance.gameObject.SetActive(true);
            Instance.StartTransition(() => SceneManager.LoadScene(sceneName));
        }
        else
        {
            Debug.LogWarning("CheckerboardTransition.Instance not found in current scene. Loading scene directly.");
            SceneManager.LoadScene(sceneName);
        }
    }

    private IEnumerator AnimateCheckerboard()
    {
        // Agrupar por columnas
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

        // Ajustar duración por speed
        float adjustedDuration = animationDuration / speed;

        // Fase 1: Animar visibles (negros) columna por columna, de izquierda a derecha
        for (int x = 0; x < columns; x++)
        {
            // Animar visibles de la columna simultáneamente (a negro)
            foreach (Image square in visibleColumns[x])
            {
                StartCoroutine(FadeIn(square, adjustedDuration));
            }

            // Esperar antes de siguiente columna
            yield return new WaitForSeconds(delayBetweenColumns);
        }

        // Esperar a que termine la animación de visibles
        yield return new WaitForSeconds(adjustedDuration);

        // Fase 2: Animar hidden (transparentes) a negro, columna por columna
        for (int x = 0; x < columns; x++)
        {
            // Animar hidden de la columna simultáneamente (a negro)
            foreach (Image square in hiddenColumns[x])
            {
                StartCoroutine(FadeIn(square, adjustedDuration));
            }

            // Esperar antes de siguiente columna
            yield return new WaitForSeconds(delayBetweenColumns);
        }

        // Esperar a que termine la animación de hidden
        yield return new WaitForSeconds(adjustedDuration);

        onComplete?.Invoke();

        // Fase 3: Deshacer transición - animar de negro a transparente, de derecha a izquierda
        for (int x = columns - 1; x >= 0; x--)
        {
            // Animar todos de la columna simultáneamente (a transparente)
            foreach (Image square in squares)
            {
                int idx = squares.IndexOf(square);
                int sx = idx % columns;
                if (sx == x)
                {
                    StartCoroutine(FadeOut(square, adjustedDuration));
                }
            }

            // Esperar antes de columna anterior
            yield return new WaitForSeconds(delayBetweenColumns);
        }

        // Esperar a que termine el fade out
        yield return new WaitForSeconds(adjustedDuration);

        // Reiniciar alphas y deshabilitar raycast para no bloquear input
        foreach (Image square in squares)
        {
            square.color = new Color(0, 0, 0, 0);
            square.raycastTarget = false;
        }

        // Desactivar el transition para liberar input
        gameObject.SetActive(false);
    }

    private IEnumerator FadeIn(Image image, float duration)
    {
        float elapsed = 0f;
        Color startColor = image.color;
        Color endColor = new Color(0, 0, 0, 1);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
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
            elapsed += Time.deltaTime;
            image.color = Color.Lerp(startColor, endColor, elapsed / duration);
            yield return null;
        }

        image.color = endColor;
    }
}