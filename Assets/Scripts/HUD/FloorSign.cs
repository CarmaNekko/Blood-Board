using System.Collections;
using TMPro;
using UnityEngine;

public class FloorSign : MonoBehaviour
{
    [SerializeField] private GameObject signPanel;
    [SerializeField] private TextMeshProUGUI signText;

    private void Start()
    {
        float savedBrightness = PlayerPrefs.GetFloat("Brightness", 0f);
        if (Mathf.Abs(savedBrightness) > 0.001f)
        {
            Options.SetBrightnessOffset(savedBrightness);

            DungeonLightingManager lightingManager = FindFirstObjectByType<DungeonLightingManager>();
            if (lightingManager != null)
            {
                lightingManager.UpdateBrightness();
            }
        }

        if (signPanel != null && signText != null)
        {
            LevelManager levelManager = FindFirstObjectByType<LevelManager>();

            if (BossCheckpointState.IsBossCheckpoint && !string.IsNullOrWhiteSpace(BossCheckpointState.BossDisplayName))
            {
                signText.text = $"Piso {LevelManager.currentLevel} - {BossCheckpointState.BossDisplayName}";
            }
            else if (levelManager != null)
            {
                signText.text = $"Piso {LevelManager.currentLevel} - Castillo Monarquico";
            }
            else
            {
                signText.text = "Tutorial - Castillo Monarquico";
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
