using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform mainCamera;

    void Start()
    {
        if (Camera.main != null)
        {
            mainCamera = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.forward);
        }
    }
}