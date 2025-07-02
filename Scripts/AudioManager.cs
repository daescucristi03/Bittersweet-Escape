using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;

    [Header("Sound Clips")]
    public AudioClip slamImpactClip;
    public AudioClip dashClip;
    public AudioClip spawnMinionClip;
    public AudioClip shootProjectileClip;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlaySlamImpact() => PlaySound(slamImpactClip);
    public void PlayDash() => PlaySound(dashClip);
    public void PlaySpawnMinion() => PlaySound(spawnMinionClip);
    public void PlayProjectile() => PlaySound(shootProjectileClip);
}