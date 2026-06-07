using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class EnemySpawnEffect : MonoBehaviour
{
    [Header("Spawn Settings")]
    public float spawnDuration = 2.0f;
    public float yOffset = -3f;

    [Header("Ajuste para enemigos sin NavMesh")]
    public float surfaceOffset = 0f;
    public LayerMask groundMask = Physics.AllLayers;

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
        Vector3 finalPos = transform.position;

        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(transform.position);
            yield return null;
            finalPos = transform.position;
            agent.enabled = false;
        }
        else
        {
            if (Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 10f, groundMask, QueryTriggerInteraction.Ignore))
            {
                finalPos.y = hit.point.y + surfaceOffset;
            }
        }

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
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
            transform.position = Vector3.Lerp(startPos, finalPos, t);
            yield return null;
        }

        transform.position = finalPos;

        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(finalPos);
            yield return null;
            if (agent.isOnNavMesh) agent.ResetPath();
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