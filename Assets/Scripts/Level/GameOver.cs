using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using BloodBoard.GameManagement;
using BloodBoard.UI;

public class GameOver : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button exitButton;

    private const string creditsSceneName = "Credits";

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

    public void ShowGameOver(bool isVictory = false)
    {
        if (deathScreen == null)
        {
            Debug.LogError("GameOver: no se ha asignado 'deathScreen'.");
            return;
        }

        deathScreen.SetActive(true);

        if (isVictory)
        {
            if (titleText != null)
            {
                titleText.text = "VENCISTE A LOS REYES";
                titleText.gameObject.SetActive(true);
            }
            if (retryButton != null)
            {
                retryButton.GetComponentInChildren<TMP_Text>().text = "Registrar Puntuación";
                retryButton.onClick.RemoveAllListeners();
                retryButton.onClick.AddListener(OnRegisterScore);
                retryButton.gameObject.SetActive(true);
            }
            if (exitButton != null)
            {
                exitButton.GetComponentInChildren<TMP_Text>().text = "Continuar";
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(GoToCredits);
                exitButton.gameObject.SetActive(true);
            }
        }
        else // Derrota
        {
            if (titleText != null)
            {
                titleText.text = "HAS CAÍDO";
                titleText.gameObject.SetActive(true);
            }
            if (retryButton != null) retryButton.gameObject.SetActive(true);
            if (exitButton != null) exitButton.gameObject.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    private void OnRegisterScore()
    {
        // Oculta esta pantalla y muestra la de registro de nombre
        if (deathScreen != null) deathScreen.SetActive(false);

        int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetCurrentScore() : 0;
        EndlessScoreInputUI.Instance?.ShowForNormalMode(finalScore);
    }

    private void GoToCredits()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(creditsSceneName);
    }

    public void OnRetryButton()
    {
        ScoreManager.Instance?.ResetCurrentScore(); // Reinicia la puntuación para la nueva partida
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMenuButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScreen");
    }
}
