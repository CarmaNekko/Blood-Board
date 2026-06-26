using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("El punto exacto donde reaparecerá el jugador. Si es nulo, se usará la posición de este objeto.")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private bool disableAfterTrigger = true;

    [Header("Gizmo")]
    [SerializeField] private Color gizmoColor = Color.cyan;
    [SerializeField] private float gizmoRadius = 1f;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered checkpoint.");
            Transform pointToSet = respawnPoint != null ? respawnPoint : transform;
            CheckpointManager.Instance.SetCheckpoint(pointToSet);

            if (disableAfterTrigger)
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform pointToDraw = respawnPoint != null ? respawnPoint : transform;

        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(pointToDraw.position, gizmoRadius);

        Gizmos.color = Color.blue;
        Vector3 forwardDirection = pointToDraw.forward * gizmoRadius * 2;
        Gizmos.DrawRay(pointToDraw.position, forwardDirection);

        if (respawnPoint != null && respawnPoint != transform)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, respawnPoint.position);
        }
    }
}