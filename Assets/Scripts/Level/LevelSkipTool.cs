using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSkipTool : MonoBehaviour
{
    public string normalLevelSceneName = "Level_1";
    public string bossLevelSceneName = "Boss_Knight";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            string currentScene = SceneManager.GetActiveScene().name;

            if (currentScene == bossLevelSceneName)
            {
                Debug.Log("Saltando escena de Jefe...");
                CheckerboardTransition.LoadScene(normalLevelSceneName);
            }
            else
            {
                LevelManager manager = Object.FindFirstObjectByType<LevelManager>();
                if (manager != null)
                {
                    Debug.Log("Saltando al siguiente nivel...");
                    manager.AdvanceLevel();
                }
            }
        }
    }
}