using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FPSManager : MonoBehaviour
{
    public static FPSManager Instance { get; private set; }
    
    private TMP_Text fpsText;
    private float deltaTime = 0.0f;
    private bool initialized = false;

    [RuntimeInitializeOnLoadMethod]
    private static void InitializeOnLoad()
    {
        // Crear el FPSManager automáticamente al cargar la escena
        if (Instance == null)
        {
            GameObject fpsManagerObj = new GameObject("FPSManager");
            FPSManager manager = fpsManagerObj.AddComponent<FPSManager>();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Escuchar cambios de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reinicializar cuando cambia de escena
        initialized = false;
        fpsText = null;
        Debug.Log($"FPSManager: Escena '{scene.name}' cargada. Reintentando búsqueda de FPS...");
    }

    private void Start()
    {
        InitializeFPS();
    }

    private void Update()
    {
        // Si aún no está inicializado, intentar de nuevo
        if (!initialized)
        {
            InitializeFPS();
        }

        if (!initialized || fpsText == null) return;

        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = $"FPS: {fps:0}";
    }

    private void InitializeFPS()
    {
        // Buscar el objeto FPS en la jerarquía
        Transform fpsTransform = FindFPSTextInHierarchy();
        if (fpsTransform != null)
        {
            fpsText = fpsTransform.GetComponent<TMP_Text>();
            if (fpsText != null)
            {
                initialized = true;
                Debug.Log("FPSManager: FPS Text encontrado e inicializado.");
            }
        }

        if (!initialized)
        {
            Debug.LogWarning("FPSManager: No se encontró el objeto FPS o su componente TMP_Text en esta escena.");
        }
    }

    private Transform FindFPSTextInHierarchy()
    {
        // Buscar en el Canvas principal
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            Transform fpsTransform = canvas.transform.Find("FPS");
            if (fpsTransform != null)
            {
                return fpsTransform;
            }

            // Buscar en todos los hijos recursivamente
            fpsTransform = RecursiveFind(canvas.transform, "FPS");
            if (fpsTransform != null)
            {
                return fpsTransform;
            }
        }
        return null;
    }

    private Transform RecursiveFind(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }
            Transform result = RecursiveFind(child, name);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }
}
