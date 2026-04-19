using UnityEngine;

public class DoorConnector : MonoBehaviour
{
    [Header("Configuración de Conexión")]
    public bool isConnected = false;

    public float doorHeightOffset = 0f;

    private void OnDrawGizmos()
    {
        Gizmos.color = isConnected ? Color.green : Color.red;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(5, 4, 0.1f));

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(Vector3.zero, Vector3.forward * 2f);
    }
}
