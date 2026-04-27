using UnityEngine;
using TMPro;
using System.Collections;

public class FloorSign : MonoBehaviour
{
    [SerializeField] private GameObject signPanel; // Asigna el Panel UI en Inspector
    [SerializeField] private TextMeshProUGUI signText; // Asigna el TextMeshProUGUI en Inspector

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
            PauseScreen.IsFloorSignActive = true; // Bloquea input de pausa
            Time.timeScale = 0f; // Pausa el juego durante el cartel
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
        Time.timeScale = 1f; // Restaura el tiempo del juego
        PauseScreen.IsFloorSignActive = false; // Permite input de pausa
        Debug.Log("Ocultando cartel");
    }
}
