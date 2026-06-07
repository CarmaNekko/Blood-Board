using System.Collections.Generic;
using UnityEngine;

public class RookProtector : MonoBehaviour
{
    [Header("Settings")]
    public float protectionRadius = 8f;
    public int maxProtectedEnemies = 3;
    public LayerMask enemyLayer;

    private List<EnemyHealth> protectedTargets = new List<EnemyHealth>();

    void Update()
    {
        protectedTargets.RemoveAll(e => e == null);

        if (protectedTargets.Count < maxProtectedEnemies)
        {
            FindNewTargets();
        }
    }

    void FindNewTargets()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, protectionRadius, enemyLayer);
        foreach (var hit in hits)
        {
            if (protectedTargets.Count >= maxProtectedEnemies) break;

            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null &&
                enemy.gameObject != gameObject &&
                enemy.GetComponent<RookProtector>() == null &&
                !protectedTargets.Contains(enemy))
            {
                enemy.SetShield(true);
                protectedTargets.Add(enemy);
            }
        }
    }

    void OnDestroy()
    {
        foreach (var enemy in protectedTargets)
        {
            if (enemy != null)
            {
                enemy.SetShield(false);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, protectionRadius);
    }
}
