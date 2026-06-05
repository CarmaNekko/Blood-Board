using UnityEngine;
using System.Collections;

public class BishopCrystal : MonoBehaviour
{
    public MagicColor crystalColor;
    [SerializeField] private Material whiteMaterial;
    [SerializeField] private Material blackMaterial;
    [SerializeField] private GameObject shieldVisual;
    [SerializeField] private LineRenderer tetherLine;

    [Header("Animaciones")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private float rotateSpeed = 45f;
    [SerializeField] private float riseHeight = 15f;
    [SerializeField] private float riseDuration = 2f;

    private BishopArenaManager arenaManager;
    private Transform bossTransform;
    private bool isVulnerable = false;
    private Vector3 startPos;
    private bool hasRisen = false;

    void Awake()
    {
        startPos = transform.position;
        transform.position = startPos + Vector3.down * riseHeight;
        if (tetherLine != null) tetherLine.enabled = false;
        if (shieldVisual != null) shieldVisual.SetActive(false);
    }

    public void Initialize(BishopArenaManager manager, Transform boss)
    {
        arenaManager = manager;
        bossTransform = boss;
        SetProtected(true);

        StartCoroutine(RiseRoutine());
    }

    private IEnumerator RiseRoutine()
    {
        float t = 0;
        Vector3 hiddenPos = transform.position;
        while (t < 1f)
        {
            t += Time.deltaTime / riseDuration;
            transform.position = Vector3.Lerp(hiddenPos, startPos, t);
            UpdateTether();
            yield return null;
        }
        hasRisen = true;
    }

    void Update()
    {
        if (hasRisen)
        {
            transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
            float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        UpdateTether();
    }

    private void UpdateTether()
    {
        if (tetherLine != null && bossTransform != null && !isVulnerable && gameObject.activeSelf)
        {
            tetherLine.enabled = true;
            tetherLine.SetPosition(0, transform.position);
            tetherLine.SetPosition(1, bossTransform.position);
        }
        else if (tetherLine != null)
        {
            tetherLine.enabled = false;
        }
    }

    public void SetProtected(bool isProtected)
    {
        isVulnerable = !isProtected;
        if (shieldVisual != null) shieldVisual.SetActive(isProtected);
        UpdateTether();
    }

    public void RandomizeColor()
    {
        crystalColor = (Random.value > 0.5f) ? MagicColor.White : MagicColor.Black;
        Renderer rend = GetComponent<Renderer>();
        if (rend != null) rend.material = (crystalColor == MagicColor.White) ? whiteMaterial : blackMaterial;
    }

    public void TakeDamage(MagicColor hitColor)
    {
        if (!isVulnerable) return;

        if (hitColor != crystalColor)
        {
            isVulnerable = false;
            gameObject.SetActive(false);
            if (tetherLine != null) tetherLine.enabled = false;
            if (arenaManager != null) arenaManager.OnCrystalDestroyed();
        }
    }
}