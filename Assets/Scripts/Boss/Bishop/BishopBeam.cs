using UnityEngine;

public class BishopBeam : MonoBehaviour
{
    [Header("Configuración del Daño")]
    [SerializeField] private float damageOnContact = 35f;

    private bool hasDealtDamage = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasDealtDamage)
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageOnContact);
                hasDealtDamage = true;
            }
        }
    }
}