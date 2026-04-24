using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    public static TitleScreen Instance { get; private set; }

    [Header("Referencias")]
    [SerializeField] private GameObject titleBackground;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button creditsButton;

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

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Start()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButton);
        }

        if (optionsButton != null)
        {
            optionsButton.onClick.AddListener(OnOptionsButton);
        }

        if (creditsButton != null)
        {
            creditsButton.onClick.AddListener(OnCreditsButton);
        }
    }

    public void OnPlayButton()
    {
        SceneManager.LoadScene("Level_Tuto");
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

    public void OnCreditsButton()
    {
        SceneManager.LoadScene("Credits");
    }
}
