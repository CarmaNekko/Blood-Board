using UnityEngine;
using System.Collections;

public class BishopBossController : MonoBehaviour
{
    [Header("Animación de Crecimiento")]
    [SerializeField] private Vector3 startScale = new Vector3(50f, 50f, 50f);
    [SerializeField] private Vector3 giantScale = new Vector3(289.309f, 289.309f, 289.309f);
    [SerializeField] private float growDuration = 3f;

    [Header("Animación de Vuelo (Infinito)")]
    [SerializeField] private float flySpeed = 1.5f;
    [SerializeField] private float flyWidth = 20f;
    [SerializeField] private float flyHeight = 5f;

    private Vector3 centerPosition;
    private float flightTime = 0f;

    public bool IsReady { get; private set; } = false;
    private bool isFlying = false;
    private bool isDead = false;

    void Awake()
    {
        centerPosition = transform.position;
        transform.localScale = startScale;
    }

    public void AppearAndGrow()
    {
        gameObject.SetActive(true);
        StartCoroutine(GrowRoutine());
    }

    private IEnumerator GrowRoutine()
    {
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / growDuration;
            transform.localScale = Vector3.Lerp(startScale, giantScale, t);
            yield return null;
        }

        transform.localScale = giantScale;
        IsReady = true;
        isFlying = true;
    }

    void Update()
    {
        if (isFlying && !isDead)
        {
            flightTime += Time.deltaTime * flySpeed;

            float newY = centerPosition.y + Mathf.Sin(flightTime * 2f) * (flyHeight / 2f);
            float newZ = centerPosition.z + Mathf.Sin(flightTime) * flyWidth;

            transform.position = new Vector3(centerPosition.x, newY, newZ);
        }

        if (isDead)
        {
            transform.position += Vector3.down * 30f * Time.deltaTime;
        }
    }

    public void DefeatAndFall()
    {
        isFlying = false;
        isDead = true;
    }
}