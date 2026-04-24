using UnityEngine;
using UnityEngine.SceneManagement;

public class BackTitleButton : MonoBehaviour
{
    [SerializeField] private string titleSceneName = "TitleScreen";

    public void BackTitle()
    {
        SceneManager.LoadScene(titleSceneName);
    }
}
