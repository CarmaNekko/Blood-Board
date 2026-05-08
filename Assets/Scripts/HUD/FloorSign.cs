using UnityEngine;
using TMPro;
using System.Collections;

public class FloorSign : MonoBehaviour
{
    [SerializeField] private GameObject signPanel;
    [SerializeField] private TextMeshProUGUI signText;

    private void Start()
    {
        if (signPanel != null && signText != null)
        {
            LevelManager levelManager = FindFirstObjectByType<LevelManager>();
            if (levelManager != null)
            {
                signText.text = $"Piso {LevelManager.currentLevel} - Castillo Monárquico";
            }
            else
            {
                signText.text = "Tutorial - Castillo Monárquico";
            }
            PauseScreen.IsFloorSignActive = true;
            Time.timeScale = 0f;
            signPanel.SetActive(true);
            Debug.Log("Mostrando cartel: " + signText.text);
            StartCoroutine(HideSign());
        }
        else
        {
            Debug.LogError("FloorSign: referencias no asignadas (signPanel o signText son null)");
        }
    }

    private IEnumerator HideSign()
    {
        yield return new WaitForSecondsRealtime(3f);
        signPanel.SetActive(false);
        Time.timeScale = 1f;
        PauseScreen.IsFloorSignActive = false;
        Debug.Log("Ocultando cartel");
    }
}
