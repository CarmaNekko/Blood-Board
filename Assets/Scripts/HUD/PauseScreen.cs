using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseScreen : MonoBehaviour
{
    public static PauseScreen Instance { get; private set; }
    public static bool IsPaused { get; private set; }
    public static bool IsFloorSignActive { get; set; } = false;

    [Header("Referencias")]
    [SerializeField] private GameObject pauseBackground;
    [SerializeField] private TMP_Text pauseTitleText;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button mainMenuButton;

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

        if (pauseBackground != null)
        {
            pauseBackground.SetActive(false);
        }

        if (pauseTitleText != null)
        {
            pauseTitleText.gameObject.SetActive(false);
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButton);
            continueButton.gameObject.SetActive(false);
        }

        if (optionsButton != null)
        {
            optionsButton.onClick.AddListener(OnOptionsButton);
            optionsButton.gameObject.SetActive(false);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuButton);
            mainMenuButton.gameObject.SetActive(false);
        }

        TutorialMessage.ResetTutorialState();
        SetPause(false);
    }

    private void Update()
    {
        bool transitionIsActive = CheckerboardTransition.Instance != null && CheckerboardTransition.Instance.IsTransitioning;

        if (!IsFloorSignActive && !transitionIsActive && Input.GetKeyDown(KeyCode.Escape))
        {
            if (Options.Instance != null && Options.Instance.IsOpen)
            {
                Options.Instance.HideOptions();
            }
            else
            {
                TogglePause();
            }
        }
    }

    public void TogglePause()
    {
        SetPause(!IsPaused);
    }

    public static void SetPause(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = IsPaused || TutorialMessage.IsTutorialActive ? 0f : 1f;

        if (Instance != null)
        {
            if (Instance.pauseBackground != null)
            {
                Instance.pauseBackground.SetActive(paused);
            }

            if (Instance.pauseTitleText != null)
            {
                Instance.pauseTitleText.gameObject.SetActive(paused);
            }

            if (Instance.continueButton != null)
            {
                Instance.continueButton.gameObject.SetActive(paused);
            }

            if (Instance.optionsButton != null)
            {
                Instance.optionsButton.gameObject.SetActive(paused);
            }

            if (Instance.mainMenuButton != null)
            {
                Instance.mainMenuButton.gameObject.SetActive(paused);
            }
        }

        Cursor.lockState = IsPaused || TutorialMessage.IsTutorialActive ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = IsPaused || TutorialMessage.IsTutorialActive;
    }

    public void OnContinueButton()
    {
        SetPause(false);
    }

    public void OnOptionsButton()
    {
        if (Options.Instance != null)
        {
            Options.Instance.ShowOptions();
        }
        else
        {
            Debug.Log("Opciones no disponibles");
        }
    }

    public void OnMainMenuButton()
    {
        TutorialMessage.ResetTutorialState();
        SetPause(false);
        Time.timeScale = 1f;
        CheckerboardTransition.directToMenu = true;
        CheckerboardTransition.LoadScene("NewTitleScreen");
    }
}
