using UnityEngine;

public class TutorialMessage : MonoBehaviour
{
    [Header("Interfaz")]
    public GameObject tutorialPanel;

    private bool hasBeenTriggered = false;

    public static bool IsTutorialActive = false;

    private void Awake()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (hasBeenTriggered && IsTutorialActive)
        {
            ResetTutorialState();
        }
    }

    public static void ResetTutorialState()
    {
        IsTutorialActive = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenTriggered)
        {
            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(true);
            }

            Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            hasBeenTriggered = true;
            IsTutorialActive = true;
        }
    }

    public void CloseTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        Time.timeScale = PauseScreen.IsPaused ? 0f : 1f;

        Cursor.lockState = PauseScreen.IsPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = PauseScreen.IsPaused;

        ResetTutorialState();
    }
}

