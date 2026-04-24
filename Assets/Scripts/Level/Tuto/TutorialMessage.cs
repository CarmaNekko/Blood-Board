using UnityEngine;

public class TutorialMessage : MonoBehaviour
{
    [Header("Interfaz")]
    public GameObject tutorialPanel;

    private bool hasBeenTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenTriggered)
        {
            tutorialPanel.SetActive(true);

            Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            hasBeenTriggered = true;
        }
    }

    public void CloseTutorial()
    {
        tutorialPanel.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
