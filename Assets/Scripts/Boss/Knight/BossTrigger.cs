using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    [Header("References")]
    public GameObject startRoom;
    public BossKnight bossKnight;

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

            gameObject.SetActive(false);
        }
    }
}