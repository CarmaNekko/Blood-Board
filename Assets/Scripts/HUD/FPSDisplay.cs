using TMPro;
using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    // Este script ahora actúa como referencia, pero el cálculo de FPS lo hace FPSManager
    // para asegurar que se actualice incluso cuando este objeto esté inactivo
    
    private void OnEnable()
    {
        // Asegurar que FPSManager existe en la escena
        if (FPSManager.Instance == null)
        {
            GameObject fpsManagerObj = new GameObject("FPSManager");
            fpsManagerObj.AddComponent<FPSManager>();
        }
    }
}
