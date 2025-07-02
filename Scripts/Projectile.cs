using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 5f;
    public GameObject hitEffectPrefab;
    public AudioClip hitSound;

    public AudioClip spawnSound;

    void Start()
    {
        Destroy(gameObject, lifetime);
        if (spawnSound != null)
                    AudioSource.PlayClipAtPoint(spawnSound, Camera.main.transform.position);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(1);
                if (hitSound != null)
                    AudioSource.PlayClipAtPoint(hitSound, Camera.main.transform.position);
            }

            PlayEffects();
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        PlayEffects();
    }

    void PlayEffects()
    {
        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

    }
}