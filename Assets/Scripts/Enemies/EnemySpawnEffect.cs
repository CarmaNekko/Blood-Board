using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class EnemySpawnEffect : MonoBehaviour
{
    [Header("Spawn Settings")]
    public float spawnDuration = 2.0f;
    public float yOffset = -3f;

    private MonoBehaviour[] scriptsToDisable;
    private NavMeshAgent agent;
    private Collider[] allColliders;
    private Rigidbody rb;
    private Renderer[] allRenderers;
    private bool wasKinematic;

    void Awake()
    {
        scriptsToDisable = GetComponents<MonoBehaviour>();
        agent = GetComponent<NavMeshAgent>();
        allColliders = GetComponentsInChildren<Collider>();
        rb = GetComponent<Rigidbody>();
        allRenderers = GetComponentsInChildren<Renderer>();

        foreach (var script in scriptsToDisable)
        {
            if (script != this) script.enabled = false;
        }

        foreach (var col in allColliders)
        {
            col.enabled = false;
        }

        if (rb != null)
        {
            wasKinematic = rb.isKinematic;
            rb.isKinematic = true;
        }

        foreach (var r in allRenderers)
        {
            r.enabled = false;
        }

    }

    IEnumerator Start()
    {
        yield return null;

        Vector3 finalPos = transform.position;

        if (agent != null) agent.enabled = false;

        Vector3 startPos = finalPos + new Vector3(0, yOffset, 0);
        transform.position = startPos;

        foreach (var r in allRenderers)
        {
            r.enabled = true;
        }

        float elapsed = 0;
        while (elapsed < spawnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spawnDuration;

            // Suavizado Ease-Out
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            transform.position = Vector3.Lerp(startPos, finalPos, t);
            yield return null;
        }

        transform.position = finalPos;

        if (agent != null)
        {
            agent.Warp(finalPos);
            agent.enabled = true;
            yield return null;
            agent.ResetPath();
        }

        foreach (var script in scriptsToDisable)
        {
            if (script != this) script.enabled = true;
        }

        foreach (var col in allColliders)
        {
            col.enabled = true;
        }

        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
        }

        Destroy(this);
    }
}