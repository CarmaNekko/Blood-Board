using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CeilingCollapseManager : MonoBehaviour
{
    [Header("Event Configuration")]
    [SerializeField] private Destruction[] pillars;
    [SerializeField] private GameObject intactCeiling;
    [SerializeField] private GameObject fracturedCeilingPrefab;
    [SerializeField] private float minDropDelay = 0.05f;
    [SerializeField] private float maxDropDelay = 0.2f;

    private bool collapseStarted = false;
    private List<Transform> availableDebris = new List<Transform>();

    private void Update()
    {
        if (!collapseStarted && CheckAllPillarsDestroyed())
        {
            collapseStarted = true;
            StartCoroutine(DropCeilingRoutine());
        }
    }

    private bool CheckAllPillarsDestroyed()
    {
        foreach (Destruction pillar in pillars)
        {
            if (pillar != null && !pillar.IsDestroyed)
            {
                return false;
            }
        }
        return true;
    }

    private IEnumerator DropCeilingRoutine()
    {
        if (intactCeiling != null) intactCeiling.SetActive(false);

        if (fracturedCeilingPrefab != null)
        {
            GameObject fracturedInstance = Instantiate(fracturedCeilingPrefab, intactCeiling.transform.position, fracturedCeilingPrefab.transform.rotation);

            foreach (Transform child in fracturedInstance.transform)
            {
                Rigidbody rb = child.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.solverIterations = 2;
                    rb.solverVelocityIterations = 1;

                    availableDebris.Add(child);
                }
            }
        }

        while (availableDebris.Count > 0)
        {
            DropRandomDebris();
            float waitTime = Random.Range(minDropDelay, maxDropDelay);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void DropRandomDebris()
    {
        int randomIndex = Random.Range(0, availableDebris.Count);
        Transform pieceToDrop = availableDebris[randomIndex];
        availableDebris.RemoveAt(randomIndex);

        if (pieceToDrop == null) return;

        pieceToDrop.localScale *= 0.95f;

        Rigidbody rb = pieceToDrop.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(Vector3.down * 5f, ForceMode.VelocityChange);
        }
    }
}