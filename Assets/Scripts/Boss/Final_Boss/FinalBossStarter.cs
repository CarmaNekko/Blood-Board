using UnityEngine;

public class FinalBossStarter : MonoBehaviour
{
    public GameObject[] barriersToActivate;
    public MonoBehaviour[] scriptsToActivate;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (GameObject barrier in barriersToActivate)
            {
                if (barrier != null)
                {
                    barrier.SetActive(true);
                }
            }

            foreach (MonoBehaviour script in scriptsToActivate)
            {
                if (script != null)
                {
                    script.enabled = true;
                }
            }

            gameObject.SetActive(false);
        }
    }
}