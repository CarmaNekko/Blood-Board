using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CreditsManager : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private Camera mainMenuCamera;
    [SerializeField] private Camera creditsCamera;

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject creditsCanvasObject;

    [Header("Transitions")]
    [SerializeField] private CheckerboardTransition transition;

    private bool isShowingCredits = false;

    public void ShowCredits()
    {
        Debug.Log($"[CreditsManager] ShowCredits called. isShowingCredits={isShowingCredits}");
        
        if (isShowingCredits) return;

        isShowingCredits = true;

        StartCoroutine(ShowCreditsWithTransition());
    }

    private IEnumerator ShowCreditsWithTransition()
    {
        Debug.Log($"[CreditsManager] ShowCreditsWithTransition. transition={transition != null}");
        
        if (transition != null)
        {
            Debug.Log("[CreditsManager] Using CheckerboardTransition");
            transition.StartTransition(ActivateCreditsUI, false);
        }
        else
        {
            Debug.Log("[CreditsManager] No transition, calling ActivateCreditsUI directly");
            ActivateCreditsUI();
        }

        yield return null;
    }

    private void ActivateCreditsUI()
    {
        Debug.Log($"[CreditsManager] ActivateCreditsUI. creditsCanvasObject={creditsCanvasObject != null}, active={creditsCanvasObject?.activeSelf}");
        
        if (mainMenuCamera != null) mainMenuCamera.gameObject.SetActive(false);
        if (creditsCamera != null) creditsCamera.gameObject.SetActive(true);

        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (creditsCanvasObject != null) creditsCanvasObject.SetActive(true);

        TitleScreenSelection.Instance?.HideMainMenu();
    }

    public void HideCredits()
    {
        if (!isShowingCredits) return;

        isShowingCredits = false;

        StartCoroutine(HideCreditsWithTransition());
    }

    private IEnumerator HideCreditsWithTransition()
    {
        if (transition != null)
        {
            transition.StartTransition(DeactivateCreditsUI, false);
        }
        else
        {
            DeactivateCreditsUI();
        }

        yield return null;
    }

    private void DeactivateCreditsUI()
    {
        if (creditsCamera != null) creditsCamera.gameObject.SetActive(false);
        if (mainMenuCamera != null) mainMenuCamera.gameObject.SetActive(true);

        if (creditsCanvasObject != null) creditsCanvasObject.SetActive(false);
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);

        TitleScreenSelection.Instance?.ShowMainMenu();
    }
}