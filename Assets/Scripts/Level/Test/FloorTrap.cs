using System.Collections;
using UnityEngine;

public class FloorTrap : MonoBehaviour
{
    [Header("Trap Settings")]
    public GameObject[] floorPieces;
    public float delayBeforeFall = 0f;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(BreakFloor());
        }
    }

    private IEnumerator BreakFloor()
    {
        if (delayBeforeFall > 0)
        {
            yield return new WaitForSeconds(delayBeforeFall);
        }

        foreach (GameObject piece in floorPieces)
        {
            Collider pieceCollider = piece.GetComponent<Collider>();
            if (pieceCollider != null)
            {
                pieceCollider.enabled = false;
            }

            Rigidbody rb = piece.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = piece.AddComponent<Rigidbody>();
            }

            rb.isKinematic = false;
            rb.useGravity = true;

            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

            Destroy(piece, 5f);
        }
    }
}
