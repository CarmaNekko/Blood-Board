using UnityEngine;
using System.Collections;

public class FinalBossStarter : MonoBehaviour
{
    public GameObject[] barriersToActivate;
    public Behaviour[] componentsToActivate;
    [SerializeField] private float activationDelay = 2.0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (GameObject barrier in barriersToActivate)
            {
                if (barrier != null) barrier.SetActive(true);
            }

            StartCoroutine(ActivateBossesWithDelay());
        }
    }

    private IEnumerator ActivateBossesWithDelay()
    {
        yield return new WaitForSeconds(activationDelay);

        foreach (Behaviour comp in componentsToActivate)
        {
            if (comp != null) comp.enabled = true;
        }

        gameObject.SetActive(false);
    }
}