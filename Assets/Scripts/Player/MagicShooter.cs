using UnityEngine;

public class MagicShooter : MonoBehaviour
{
    [Header("Projectiles")]
    [SerializeField] private GameObject whiteMagicPrefab;
    [SerializeField] private GameObject blackMagicPrefab;

    [Header("Shooting Setup")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootForce = 30f;

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot(whiteMagicPrefab);
        }
        else if (Input.GetButtonDown("Fire2"))
        {
            Shoot(blackMagicPrefab);
        }
    }

    private void Shoot(GameObject magicPrefab)
    {
        if (magicPrefab != null)
        {
            GameObject projectile = Instantiate(magicPrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = firePoint.forward * shootForce;
            }

            Destroy(projectile, 2f);
        }
    }
}
