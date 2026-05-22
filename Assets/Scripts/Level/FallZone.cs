using UnityEngine;

public class FallZone : MonoBehaviour
{
    private GameOver gameOverManager;

    void Start()
    {
        gameOverManager = FindAnyObjectByType<GameOver>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (gameOverManager != null)
            {
                gameOverManager.ShowGameOver(false);
            }
        }
    }
}