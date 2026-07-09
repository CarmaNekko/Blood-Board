using UnityEngine;

public class WallTrap : MonoBehaviour
{
    public Transform leftWall;
    public Transform rightWall;

    public float closingDistance = 4f;
    public float speed = 2f;

    private Vector3 leftStart;
    private Vector3 rightStart;
    private Vector3 leftTarget;
    private Vector3 rightTarget;

    private bool isPlayerInside = false;

    public enum TrapState { Abierta, Cerrando, Esperando, Abriendo }
    private TrapState estadoActual = TrapState.Abierta;

    void Start()
    {
        leftStart = leftWall.position;
        rightStart = rightWall.position;

        leftTarget = leftStart + (leftWall.forward * closingDistance);
        rightTarget = rightStart + (rightWall.forward * closingDistance);
    }

    void Update()
    {
        switch (estadoActual)
        {
            case TrapState.Cerrando:
                leftWall.position = Vector3.MoveTowards(leftWall.position, leftTarget, speed * Time.deltaTime);
                rightWall.position = Vector3.MoveTowards(rightWall.position, rightTarget, speed * Time.deltaTime);

                if (leftWall.position == leftTarget && rightWall.position == rightTarget)
                {
                    estadoActual = isPlayerInside ? TrapState.Esperando : TrapState.Abriendo;
                }
                break;

            case TrapState.Esperando:
                if (!isPlayerInside)
                {
                    estadoActual = TrapState.Abriendo;
                }
                break;

            case TrapState.Abriendo:
                leftWall.position = Vector3.MoveTowards(leftWall.position, leftStart, speed * Time.deltaTime);
                rightWall.position = Vector3.MoveTowards(rightWall.position, rightStart, speed * Time.deltaTime);

                if (leftWall.position == leftStart && rightWall.position == rightStart)
                {
                    estadoActual = TrapState.Abierta;
                }
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;

            if (estadoActual == TrapState.Abierta || estadoActual == TrapState.Abriendo)
            {
                estadoActual = TrapState.Cerrando;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }
}