using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float chaseRange = 5f;
    public float wanderInterval = 2f;

    [Header("Death FX")]
    public GameObject deathEffectPrefab;
    public AudioClip popSound;

    private Transform player;
    private Rigidbody2D rb;
    private Vector2 wanderDirection;
    private float wanderTimer;

    private enum State { Wandering, Chasing }
    private State currentState = State.Wandering;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        ChooseWanderDirection();
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= chaseRange)
            currentState = State.Chasing;
        else
            currentState = State.Wandering;
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case State.Wandering:
                Wander();
                break;
            case State.Chasing:
                ChasePlayer();
                break;
        }
    }

    void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
    }

    void Wander()
    {
        wanderTimer -= Time.fixedDeltaTime;
        if (wanderTimer <= 0f)
        {
            ChooseWanderDirection();
        }

        rb.MovePosition(rb.position + wanderDirection * moveSpeed * 0.5f * Time.fixedDeltaTime);
    }

    void ChooseWanderDirection()
    {
        wanderDirection = Random.insideUnitCircle.normalized;
        wanderTimer = wanderInterval;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement movement = other.GetComponent<PlayerMovement>();
            PlayerHealth health = other.GetComponent<PlayerHealth>();

            if (movement != null && movement.IsDashing())
            {
                // Player is dashing – enemy takes damage and dies
                if (deathEffectPrefab != null)
                    Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

                if (popSound != null)
                    AudioSource.PlayClipAtPoint(popSound, Camera.main.transform.position);

                Destroy(gameObject);
            }
            else if (health != null)
            {
                // Player is not dashing – they take damage
                health.TakeDamage(1);

                // Spawn puff effect
                if (deathEffectPrefab != null)
                    Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

                // Play pop sound
                if (popSound != null)
                    AudioSource.PlayClipAtPoint(popSound, Camera.main.transform.position);

                Destroy(gameObject);
            }
        }
    }
}