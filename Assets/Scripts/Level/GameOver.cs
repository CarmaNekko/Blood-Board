using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button exitButton;

    private void Start()
    {
        if (deathScreen != null)
        {
            deathScreen.SetActive(false);
        }

        if (titleText != null)
        {
            titleText.gameObject.SetActive(false);
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryButton);
            retryButton.gameObject.SetActive(false);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnMenuButton);
            exitButton.gameObject.SetActive(false);
        }
    }

    public void ShowGameOver()
    {
        if (deathScreen == null)
        {
            Debug.LogError("GameOver: no se ha asignado 'deathScreen'.");
            return;
        }

        deathScreen.SetActive(true);

        if (titleText != null)
        {
            titleText.gameObject.SetActive(true);
        }

        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(true);
        }

        if (exitButton != null)
        {
            exitButton.gameObject.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    public void OnRetryButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMenuButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScreen");
    }
}
