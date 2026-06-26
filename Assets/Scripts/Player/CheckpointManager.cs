using System.Collections;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("Configuración de Respawn")]
    [SerializeField] private float respawnFadeDuration = 0.5f;
    [SerializeField] private string playerTag = "Player";

    private Vector3? currentRespawnPoint;
    private Quaternion currentRespawnRotation;
    private CheckerboardTransition transitionEffect;
    private bool isRespawning = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        transitionEffect = FindObjectOfType<CheckerboardTransition>();
        if (transitionEffect == null)
        {
            Debug.LogError("No se encontró un CheckerboardTransition en la escena. El respawn no tendrá efecto de fundido.");
        }
    }

    public void SetCheckpoint(Transform respawnTransform)
    {
        if (respawnTransform != null)
        {
            currentRespawnPoint = respawnTransform.position;
            currentRespawnRotation = respawnTransform.rotation;
            Debug.Log($"Checkpoint actualizado a: {currentRespawnPoint.Value}");
        }
    }

    public void TriggerRespawn()
    {
        if (isRespawning) return;

        if (transitionEffect != null && transitionEffect.IsTransitioning)
        {
            Debug.Log("Respawn blocked: a scene transition is in progress.");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            Debug.LogError("No se pudo encontrar al jugador con el tag: " + playerTag);
            return;
        }

        if (currentRespawnPoint.HasValue)
        {
            StartCoroutine(RespawnRoutine(player));
        }
        else
        {
            Debug.LogWarning("Se intentó hacer respawn pero no hay ningún checkpoint activo.");
        }
    }

    private IEnumerator RespawnRoutine(GameObject player)
    {
        isRespawning = true;
        try
        {
            if (transitionEffect != null)
            {
                yield return transitionEffect.StartFadeToBlack(respawnFadeDuration);
            }

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = currentRespawnPoint.Value;
            player.transform.rotation = currentRespawnRotation;

            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null) pm.ResetVelocity();

            if (cc != null) cc.enabled = true;

            if (transitionEffect != null)
            {
                yield return transitionEffect.StartFadeFromBlack(respawnFadeDuration);
            }
        }
        finally
        {
            isRespawning = false;
        }
    }
}