using UnityEngine;

public class DoorConnector : MonoBehaviour
{
    [Header("Configuracion de Conexion")]
    public bool isConnected = false;
    public float doorHeightOffset = 0f;

    [Header("Bloqueo de Arena")]
    public GameObject blockerObject;

    [HideInInspector] public int owningRoomNodeId = -1;
    [HideInInspector] public int sourceRoomNodeId = -1;

    public void SetLock(bool isLocked)
    {
        if (blockerObject != null)
        {
            blockerObject.SetActive(isLocked);
        }
    }

    public int GetSourceRoomNodeId()
    {
        if (sourceRoomNodeId >= 0)
            return sourceRoomNodeId;
        return owningRoomNodeId;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isConnected ? Color.green : Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(5f, 4f, 0.1f));
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(Vector3.zero, Vector3.forward * 2f);
    }
}