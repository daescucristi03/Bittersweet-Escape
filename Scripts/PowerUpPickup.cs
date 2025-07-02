using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PowerUpPickup : MonoBehaviour
{
    public float lifeTime = 15f;
    public int   healAmt  = 1;

    private PowerUpSpawner owner;

    void Start() => Invoke(nameof(AutoDespawn), lifeTime);

    public void SetOwningSpawner(PowerUpSpawner spawner) => owner = spawner;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (other.TryGetComponent<PlayerHealth>(out var ph))
            ph.Heal(healAmt);

        Collect();
    }

    void AutoDespawn() => Collect();

    void Collect()
    {
        owner?.Deregister(gameObject);
        Destroy(gameObject);
    }
}