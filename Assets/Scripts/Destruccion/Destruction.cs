using UnityEngine;
using System.Collections.Generic;

public class Destruction : MonoBehaviour
{
    [Header("Configuración de Destrucción Local")]
    public Material transparentMaterial;
    [SerializeField] private float neighborTolerance = 0.05f;

    private List<Transform> activePieces = new List<Transform>();
    private Dictionary<Transform, List<Transform>> adjacencyList = new Dictionary<Transform, List<Transform>>();
    private List<Transform> anchors = new List<Transform>();

    private void Start()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<MeshCollider>() != null && child.GetComponent<Rigidbody>() != null)
            {
                activePieces.Add(child);
                adjacencyList[child] = new List<Transform>();
            }
        }

        foreach (Transform piece in activePieces)
        {
            MeshCollider mc = piece.GetComponent<MeshCollider>();
            Bounds bounds = mc.bounds;
            Vector3 extents = bounds.extents + new Vector3(neighborTolerance, neighborTolerance, neighborTolerance);

            Collider[] hits = Physics.OverlapBox(bounds.center, extents, Quaternion.identity);
            bool isAnchor = false;

            foreach (Collider hit in hits)
            {
                if (hit == mc || hit.isTrigger || hit.CompareTag("Player")) continue;

                if (hit.transform.IsChildOf(transform))
                {
                    if (activePieces.Contains(hit.transform))
                    {
                        if (!adjacencyList[piece].Contains(hit.transform))
                        {
                            adjacencyList[piece].Add(hit.transform);
                        }
                    }
                }
                else
                {
                    Rigidbody hitRb = hit.attachedRigidbody;
                    if (hitRb == null || hitRb.isKinematic)
                    {
                        isAnchor = true;
                    }
                }
            }

            if (isAnchor)
            {
                anchors.Add(piece);
            }
        }
    }

    public virtual void DamageAtPoint(Vector3 impactPoint, float radius, float force)
    {
        bool structureChanged = false;

        for (int i = activePieces.Count - 1; i >= 0; i--)
        {
            Transform piece = activePieces[i];
            float distance = Vector3.Distance(piece.position, impactPoint);

            if (distance <= radius)
            {
                BreakPiece(piece, impactPoint, radius, force);
                activePieces.RemoveAt(i);
                structureChanged = true;
            }
        }

        if (structureChanged)
        {
            CheckFloatingIslands();
        }
    }

    private void CheckFloatingIslands()
    {
        HashSet<Transform> visited = new HashSet<Transform>();
        Queue<Transform> queue = new Queue<Transform>();

        foreach (Transform anchor in anchors)
        {
            if (activePieces.Contains(anchor))
            {
                visited.Add(anchor);
                queue.Enqueue(anchor);
            }
        }

        while (queue.Count > 0)
        {
            Transform current = queue.Dequeue();

            foreach (Transform neighbor in adjacencyList[current])
            {
                if (activePieces.Contains(neighbor) && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        for (int i = activePieces.Count - 1; i >= 0; i--)
        {
            Transform piece = activePieces[i];
            if (!visited.Contains(piece))
            {
                BreakPiece(piece, piece.position + Vector3.up * 0.5f, 1f, 10f);
                activePieces.RemoveAt(i);
            }
        }
    }

    protected void BreakPiece(Transform piece, Vector3 explosionPos, float radius, float force)
    {
        MeshCollider mc = piece.GetComponent<MeshCollider>();
        if (mc != null) mc.convex = true;

        Rigidbody rb = piece.GetComponent<Rigidbody>();
        if (rb != null && rb.isKinematic)
        {
            rb.isKinematic = false;
            if (force > 0f)
            {
                rb.AddExplosionForce(force, explosionPos, radius);
            }

            FadingDebris debrisScript = piece.gameObject.GetComponent<FadingDebris>();
            if (debrisScript == null)
            {
                debrisScript = piece.gameObject.AddComponent<FadingDebris>();
            }

            debrisScript.transparentMaterial = transparentMaterial;
            debrisScript.BeginFadeProcess();
        }
    }

    public virtual void ShatterFull(Vector3 explosionPos, float force)
    {
        Collider mainCollider = GetComponent<Collider>();
        if (mainCollider != null) mainCollider.enabled = false;

        for (int i = activePieces.Count - 1; i >= 0; i--)
        {
            BreakPiece(activePieces[i], explosionPos, 10f, force);
        }

        activePieces.Clear();
    }
}