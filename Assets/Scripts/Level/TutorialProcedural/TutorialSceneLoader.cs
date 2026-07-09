using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialSceneLoader : MonoBehaviour
{
    [Header("Cambio de escena")]
    [SerializeField] private string sceneToLoad = "Tutorial_Procedural";

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    private bool loading;

    private void OnTriggerEnter(Collider other)
    {
        if (loading)
        {
            return;
        }

        if (!IsPlayer(other))
        {
            return;
        }

        loading = true;
        SceneManager.LoadScene(sceneToLoad);
    }

    private bool IsPlayer(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        if (other.CompareTag(playerTag))
        {
            return true;
        }

        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag(playerTag))
        {
            return true;
        }

        return false;
    }
}