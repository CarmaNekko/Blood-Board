using UnityEngine;

public class WallBlocker : MonoBehaviour
{
    [Header("Objeto a Encender")]
    public GameObject invisibleWall;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (invisibleWall != null)
            {
                invisibleWall.SetActive(true);
            }
            gameObject.SetActive(false);
        }
    }
}