using UnityEngine;

public class DamageOnDash : MonoBehaviour
{
    public int damageTaken = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null && player.IsDashing())
        {
            EnemyHealth health = GetComponent<EnemyHealth>();
            if (health != null)
                health.TakeDamage(damageTaken);
        }
    }
}