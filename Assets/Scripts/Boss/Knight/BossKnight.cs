using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossKnight : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public Transform redShadow;
    public Transform playerCamera;
    public GameOver gameOverManager;

    [Header("Audio")]
    public AudioSource bossAudioSource;
    public AudioClip jumpSound;
    public AudioClip crashSound;

    [Header("Chase Stats")]
    public float distanceBehindPlayer = 5f;
    public float restTime = 0.5f;
    public float jumpDuration = 0.6f;
    public float jumpHeight = 8f;
    public float killRadius = 3.5f;
    public float shadowYOffset = 0.5f;

    [Header("Impact Effects")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.4f;

    private bool isAwake = false;
    private float finalDoorZ = Mathf.Infinity;

    private Vector3 currentStartPos;
    private Vector3 currentTargetPos;

    void Update()
    {
        if (isAwake && playerTransform.position.z < transform.position.z - 2f)
        {
            if (gameOverManager != null)
            {
                gameOverManager.ShowGameOver(false);
            }
            isAwake = false;
        }
    }

    public void WakeUp()
    {
        isAwake = true;
        redShadow.gameObject.SetActive(false);
        StartCoroutine(ChaseRoutine());
    }

    public void SetFinalDoor(float zPosition)
    {
        finalDoorZ = zPosition;
    }

    private IEnumerator ChaseRoutine()
    {
        Collider bossCollider = GetComponent<Collider>();

        while (isAwake)
        {
            yield return new WaitForSeconds(restTime);

            float newZ = Mathf.Max(transform.position.z + 5f, playerTransform.position.z - distanceBehindPlayer);

            bool isFinalJump = false;
            if (newZ >= finalDoorZ)
            {
                newZ = finalDoorZ;
                isFinalJump = true;
            }

            currentTargetPos = new Vector3(0, transform.position.y, newZ);

            redShadow.position = new Vector3(0, shadowYOffset, newZ);
            redShadow.gameObject.SetActive(true);

            yield return new WaitForSeconds(0.4f);

            if (bossAudioSource != null && jumpSound != null)
            {
                bossAudioSource.PlayOneShot(jumpSound);
            }

            if (bossCollider != null) bossCollider.isTrigger = true;

            currentStartPos = transform.position;
            float elapsedTime = 0f;

            while (elapsedTime < jumpDuration)
            {
                elapsedTime += Time.deltaTime;
                float percentage = elapsedTime / jumpDuration;

                Vector3 currentPos = Vector3.Lerp(currentStartPos, currentTargetPos, percentage);
                currentPos.y = currentStartPos.y + (Mathf.Sin(percentage * Mathf.PI) * jumpHeight);

                transform.position = currentPos;
                yield return null;
            }

            transform.position = currentTargetPos;
            redShadow.gameObject.SetActive(false);

            if (bossCollider != null) bossCollider.isTrigger = false;

            if (bossAudioSource != null && crashSound != null)
            {
                bossAudioSource.PlayOneShot(crashSound);
            }

            if (playerCamera != null)
            {
                StartCoroutine(ShakeCamera());
            }

            Vector2 bossPos2D = new Vector2(transform.position.x, transform.position.z);
            Vector2 playerPos2D = new Vector2(playerTransform.position.x, playerTransform.position.z);

            if (Vector2.Distance(bossPos2D, playerPos2D) <= killRadius)
            {
                if (gameOverManager != null)
                {
                    gameOverManager.ShowGameOver(false);
                }
                isAwake = false;
            }

            if (isFinalJump)
            {
                isAwake = false;
            }
        }
    }

    private IEnumerator ShakeCamera()
    {
        Vector3 originalPos = playerCamera.localPosition;
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            float x = originalPos.x + Random.Range(-1f, 1f) * shakeMagnitude;
            float y = originalPos.y + Random.Range(-1f, 1f) * shakeMagnitude;

            playerCamera.localPosition = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerCamera.localPosition = originalPos;
    }

    public void ResetBossPosition(float distance)
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - distance);
        redShadow.position = new Vector3(redShadow.position.x, redShadow.position.y, redShadow.position.z - distance);

        currentStartPos.z -= distance;
        currentTargetPos.z -= distance;

        if (finalDoorZ != Mathf.Infinity)
        {
            finalDoorZ -= distance;
        }
    }
}