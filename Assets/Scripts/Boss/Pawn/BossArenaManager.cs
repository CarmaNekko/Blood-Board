using UnityEngine;
using System.Collections;

public class BossArenaManager : MonoBehaviour
{
    [Header("Configuración de la Arena")]
    [SerializeField] private GameObject entranceGate;
    [SerializeField] private GameObject exitGate;
    [SerializeField] private GameObject bossHealthUI;

    [Header("Referencias del Jefe")]
    [SerializeField] private Transform bossTransform;
    [SerializeField] private PawnBossController bossController;
    [SerializeField] private PawnBossHealth bossHealth;

    [Header("Animación de Entrada/Salida")]
    [SerializeField] private float hiddenYOffset = -12f;
    [SerializeField] private float moveSpeed = 2f;

    private bool battleStarted = false;
    private bool battleEnded = false;
    private Vector3 visiblePosition;
    private Vector3 hiddenPosition;

    void Start()
    {
        if (bossHealthUI != null) bossHealthUI.SetActive(false);
        if (entranceGate != null) entranceGate.SetActive(false);
        if (exitGate != null) exitGate.SetActive(true);

        visiblePosition = bossTransform.position;
        hiddenPosition = visiblePosition + new Vector3(0, hiddenYOffset, 0);

        bossTransform.position = hiddenPosition;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !battleStarted)
        {
            battleStarted = true;
            StartCoroutine(StartBattleSequence());
        }
    }

    private IEnumerator StartBattleSequence()
    {
        if (entranceGate != null) entranceGate.SetActive(true);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * moveSpeed;
            bossTransform.position = Vector3.Lerp(hiddenPosition, visiblePosition, t);
            yield return null;
        }
        bossTransform.position = visiblePosition;

        yield return new WaitForSeconds(1.5f);
        if (bossHealthUI != null) bossHealthUI.SetActive(true);

        yield return new WaitForSeconds(1.5f);
        if (bossController != null) bossController.SetupIntroVisuals();

        yield return new WaitForSeconds(1.5f);
        if (bossController != null) bossController.StartBattle();
    }

    void Update()
    {
        if (battleStarted && !battleEnded && bossHealth != null && bossHealth.isDead)
        {
            battleEnded = true;
            StartCoroutine(EndBattleSequence());
        }
    }

    private IEnumerator EndBattleSequence()
    {
        if (bossHealthUI != null) bossHealthUI.SetActive(false);

        yield return new WaitForSeconds(1f);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * moveSpeed;
            bossTransform.position = Vector3.Lerp(visiblePosition, hiddenPosition, t);
            yield return null;
        }

        if (exitGate != null) exitGate.SetActive(false);

        if (bossTransform != null) Destroy(bossTransform.gameObject);
    }
}