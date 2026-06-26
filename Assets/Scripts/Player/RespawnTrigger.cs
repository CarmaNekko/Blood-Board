using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RespawnTrigger : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered respawn trigger. Initiating respawn.");
            CheckpointManager.Instance.TriggerRespawn();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Collider col = GetComponent<Collider>();
        if (col is BoxCollider box)
        {
            Gizmos.DrawCube(box.center, box.size);
        }
        else
        {
            Gizmos.DrawCube(Vector3.zero, col.bounds.size);
        }
    }
}