using TMPro;
using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    public static FPSDisplay Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        ApplySavedVisibility();

        if (FPSManager.Instance == null)
        {
            GameObject fpsManagerObj = new GameObject("FPSManager");
            fpsManagerObj.AddComponent<FPSManager>();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void SetVisible(bool isVisible)
    {
        ApplyVisibility(isVisible);
    }

    private void ApplySavedVisibility()
    {
        bool showFPS = PlayerPrefs.GetInt("ShowFPS", 1) == 1;
        ApplyVisibility(showFPS);
    }

    private void ApplyVisibility(bool isVisible)
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = isVisible ? 1f : 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
}
