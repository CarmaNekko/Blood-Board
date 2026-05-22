using UnityEngine;
using System.Collections;

public class ProjectileTravel : MonoBehaviour
{
    public float arcHeight = 5f;

    public void Setup(Vector3 target, float duration)
    {
        StartCoroutine(TravelRoutine(target, duration));
    }

    private IEnumerator TravelRoutine(Vector3 target, float duration)
    {
        Vector3 startPosition = transform.position;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 currentPos = Vector3.Lerp(startPosition, target, t);
            currentPos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;

            transform.position = currentPos;

            transform.LookAt(target);

            yield return null;
        }

        Destroy(gameObject);
    }
}