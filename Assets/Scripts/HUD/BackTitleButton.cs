using UnityEngine;
using UnityEngine.SceneManagement;

public class BackTitleButton : MonoBehaviour
{
    [SerializeField] private string titleSceneName = "NewTitleScreen";

    public void BackTitle()
    {
        CheckerboardTransition.LoadScene(titleSceneName);
    }
}
