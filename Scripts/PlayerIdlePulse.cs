using UnityEngine;

public class PlayerIdlePulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [Tooltip("How large the sprite gets at the top of the pulse (1.0 = no change).")]
    public float maxScaleMultiplier = 1.07f;
    [Tooltip("Seconds for one full grow-and-shrink cycle.")]
    public float pulsePeriod        = 1.2f;
    [Tooltip("Optional offset so several players don’t pulse in perfect sync.")]
    public float phaseOffset        = 0f;

    Vector3 baseScale;

    void Start() => baseScale = transform.localScale;

    void Update()
    {
        float t = (Mathf.Sin((Time.time + phaseOffset) * (Mathf.PI * 2f) / pulsePeriod) + 1f) * 0.5f;

        float scale = Mathf.Lerp(1f, maxScaleMultiplier, t);
        transform.localScale = baseScale * scale;
    }
}