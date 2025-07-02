using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HealthPowerUp : MonoBehaviour
{
    [Header("Power-Up")]
    public int healAmount   = 2;
    public float lifeTime   = 15f;
    public GameObject vfx;
    public AudioClip sfx;

    private PowerUpSpawner spawner;

    public void SetOwningSpawner(PowerUpSpawner owner) => spawner = owner;

    void Start() => Invoke(nameof(Despawn), lifeTime);

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (other.TryGetComponent<PlayerHealth>(out var hp))
            hp.Heal(healAmount);

        if (vfx) Instantiate(vfx, transform.position, Quaternion.identity);
        if (sfx) AudioSource.PlayClipAtPoint(sfx, transform.position);

        Collect();
    }

    void Despawn() => Collect();

    void Collect()
    {
        spawner?.Deregister(gameObject);
        Destroy(gameObject);
    }
}