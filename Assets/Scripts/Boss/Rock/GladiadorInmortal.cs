using UnityEngine;

public class GladiadorInmortal : MonoBehaviour
{
    void Start()
    {
        GetComponent<EnemyHealth>().SetShield(true);
    }
}