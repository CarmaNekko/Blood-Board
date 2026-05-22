using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    [Header("References")]
    public GameObject startRoom;
    public BossKnight bossKnight;
    public GameObject progressBarUI;

    private void Start()
    {
        if (progressBarUI != null)
        {
            progressBarUI.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (bossKnight != null)
            {
                bossKnight.WakeUp();
            }

            if (startRoom != null)
            {
                Destroy(startRoom, 3f);
            }

            if (progressBarUI != null)
            {
                progressBarUI.SetActive(true);
            }

            gameObject.SetActive(false);
        }
    }
}