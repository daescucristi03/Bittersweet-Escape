using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public Rigidbody2D rb;

    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool isDashing = false;
    private bool canDash = true;
    private Vector2 dashDirection;

    private TrailRenderer trail;
    private Vector2 movement;

    private bool isKnockedBack = false;

    [Header("Dash-Damage Settings")]
    [SerializeField] private LayerMask dashDamageMask;
    [SerializeField] private float dashDamageRadius = .8f;
    private bool hasDamagedDuringDash = false;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip dashClip;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        trail = GetComponent<TrailRenderer>();
        if (trail != null)
            trail.emitting = false;
    }

    void Update()
    {
        if (!isDashing && !isKnockedBack)
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
            movement = movement.normalized;
        }

        if (Input.GetKeyDown(KeyCode.Space) && canDash && movement != Vector2.zero)
        {
            dashDirection = movement;
            StartCoroutine(Dash());
        }
    }

    void FixedUpdate()
    {
        if (!isDashing && !isKnockedBack)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
        else if (isDashing)
        {
            TryDamageDuringDash();
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        hasDamagedDuringDash = false;

        if (trail != null) trail.emitting = true;

        if (audioSource && dashClip)
            audioSource.PlayOneShot(dashClip);

        float startTime = Time.time;
        while (Time.time < startTime + dashDuration)
        {
            rb.MovePosition(rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        if (trail != null) trail.emitting = false;

        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public IEnumerator ApplyKnockback(Vector2 direction, float force)
    {
        isKnockedBack = true;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * force, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.25f);
        rb.linearVelocity = Vector2.zero;
        isKnockedBack = false;
    }

    void TryDamageDuringDash()
    {
        if (hasDamagedDuringDash) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            rb.position,
            dashDamageRadius,
            dashDamageMask);

        foreach (Collider2D hit in hits)
        {
            // --- Enemy ---
            var enemy = hit.GetComponent<EnemyHealth>() ?? hit.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(1);
                hasDamagedDuringDash = true;
                return;
            }

            // --- Boss ---
            var boss = hit.GetComponent<BossHealth>() ?? hit.GetComponentInParent<BossHealth>();
            if (boss != null)
            {
                boss.TakeDamage(1);
                hasDamagedDuringDash = true;
                return;
            }
        }
    }

    public bool IsDashing()
    {
        return isDashing;
    }
}