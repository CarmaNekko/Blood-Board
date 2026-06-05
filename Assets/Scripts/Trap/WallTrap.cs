using UnityEngine;

public class WallTrap : MonoBehaviour
{
    public Transform leftWall;
    public Transform rightWall;

    public float closingDistance = 4f;
    public float speed = 2f;

    private Vector3 leftTarget;
    private Vector3 rightTarget;
    private bool isTrapActivated = false;

    void Start()
    {
        leftTarget = leftWall.position + (leftWall.forward * closingDistance);
        rightTarget = rightWall.position + (rightWall.forward * closingDistance);
    }

    void Update()
    {
        if (isTrapActivated)
        {
            leftWall.position = Vector3.MoveTowards(leftWall.position, leftTarget, speed * Time.deltaTime);
            rightWall.position = Vector3.MoveTowards(rightWall.position, rightTarget, speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTrapActivated)
        {
            isTrapActivated = true;
        }
    }
}