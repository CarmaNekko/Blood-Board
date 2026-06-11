using UnityEngine;

public class PawnShield : MonoBehaviour
{
    public MagicColor shieldColor;
    [SerializeField] private Material whiteMaterial;
    [SerializeField] private Material blackMaterial;
    [SerializeField] private GameObject shieldVisual;
    [SerializeField] private Collider shieldCollider;

    private EnemyHealth parentHealth;

    void Start()
    {
        Renderer rend = shieldVisual.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material = (shieldColor == MagicColor.White) ? whiteMaterial : blackMaterial;
        }

        parentHealth = GetComponentInParent<EnemyHealth>();
        if (parentHealth != null)
        {
            parentHealth.SetShield(true);
        }
    }

    public void TakeDamage(MagicColor hitColor)
    {
        if (hitColor != shieldColor)
        {
            shieldVisual.SetActive(false);
            shieldCollider.enabled = false;

            if (parentHealth != null)
            {
                parentHealth.SetShield(false);
            }
        }
    }
}