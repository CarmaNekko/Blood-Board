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
            scoreText.text = $"{ScoreManager.Instance.GetCurrentScore()}";
        }
        else if (scoreText != null)
        {
            scoreText.text = "0";
        }
    }
}