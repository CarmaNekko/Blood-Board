using UnityEngine;

public class DebugVictoryTrigger : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            TriggerVictory();
        }
    }

    private void TriggerVictory()
    {
        var gameOver = UnityEngine.Object.FindFirstObjectByType<GameOver>();
        if (gameOver != null)
        {
            gameOver.ShowGameOver(true);
        }
        else
        {
            Debug.LogWarning("GameOver not found in scene - cannot trigger victory");
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), "Press Keypad + to trigger victory (Debug)");
    }
}