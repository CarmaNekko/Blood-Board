using UnityEngine;

public class FPSInitializer : MonoBehaviour
{
    private void Awake()
    {
        Transform fpsTransform = transform.Find("FPS");
        if (fpsTransform == null)
        {
            Debug.LogError("FPSInitializer: No se encontró objeto 'FPS' como hijo.");
            return;
        }

        GameObject fpsObject = fpsTransform.gameObject;

        if (fpsObject.GetComponent<FPSDisplay>() == null)
        {
            FPSDisplay fpsDisplay = fpsObject.AddComponent<FPSDisplay>();
            Debug.Log("FPSInitializer: FPSDisplay agregado automáticamente al objeto FPS.");
        }
    }
}
