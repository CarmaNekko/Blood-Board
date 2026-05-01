using UnityEngine;
using TMPro;
using BloodBoard.GameManagement;

public class ScoreHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    void Update()
    {
        if (scoreText != null && ScoreManager.Instance != null)
        {
            scoreText.text = $"Puntuación: {ScoreManager.Instance.GetCurrentScore()}";
        }
        else if (scoreText != null)
        {
            scoreText.text = "Puntuación: 0";
        }
    }
}