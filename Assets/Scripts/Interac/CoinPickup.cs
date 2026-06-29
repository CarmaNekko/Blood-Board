using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [Header("Ajustes de Atracción")]
    [Tooltip("Segundos obligatorios que la moneda estará en el suelo antes de poder ser atraída")]
    public float activationDelay = 0.8f; 
    public float detectionRadius = 3.0f;
    public float timeUntilAutoPickup = 5.0f;
    public float speed = 15.0f;
    public int coinValue = 10;
    
    [Header("Audio")]
    public AudioClip collectSound;
    public float collectSoundVolume = 1.0f;
    
    [Header("Posición de Destino")]
    public float targetHeightOffset = 0.5f;
    public float collectionRadius = 1.5f; 

    private Transform player;
    private float timer = 0f;
    private bool isAttracted = false;
    private bool pointsAwarded = false;
    
    private ParticleSystem pSystem;
    private ParticleSystem.Particle[] particles; 

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        pSystem = GetComponentInChildren<ParticleSystem>();
        
        if (pSystem != null)
        {
            particles = new ParticleSystem.Particle[pSystem.main.maxParticles];
        }
    }

    void Update()
    {
        if (player == null || pSystem == null) return;

        timer += Time.deltaTime;

        if (timer < activationDelay) return; 

        float distToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distToPlayer <= detectionRadius || timer >= timeUntilAutoPickup)
        {
            isAttracted = true;
        }

        if (isAttracted)
        {
            Vector3 target = player.position + Vector3.up * targetHeightOffset;
            int numParticlesAlive = pSystem.GetParticles(particles);

            for (int i = 0; i < numParticlesAlive; i++)
            {
                particles[i].velocity = Vector3.zero;
                particles[i].position = Vector3.MoveTowards(particles[i].position, target, speed * Time.deltaTime);

                if (Vector3.Distance(particles[i].position, target) < collectionRadius)
                {
                    if (!pointsAwarded)
                    {
                        CoinManager.Instance.AddCoins(coinValue);
                        pointsAwarded = true;
                        
                        if (collectSound != null)
                        {
                            AudioSource.PlayClipAtPoint(collectSound, transform.position, collectSoundVolume);
                        }
                    }
                    particles[i].remainingLifetime = 0;
                }
            }
            
            pSystem.SetParticles(particles, numParticlesAlive);

            if (numParticlesAlive == 0 && pointsAwarded)
            {
                Destroy(gameObject);
            }
        }
    }
}