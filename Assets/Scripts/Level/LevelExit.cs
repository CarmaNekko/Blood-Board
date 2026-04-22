using UnityEngine;

public class LevelExit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LevelManager manager = Object.FindFirstObjectByType<LevelManager>();
            if (manager != null)
            {
                manager.AdvanceLevel();
            }
        }
    }
}