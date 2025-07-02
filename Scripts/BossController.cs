using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossController : MonoBehaviour
{
    public enum BossMode
    {
        Dash,
        ProjectileFan,
        ProjectileAttack,
        ProjectileRain,
        MinionSpawn,
        SlamAttack
    }

    [System.Serializable]
    public class BossAttackEntry
    {
        public BossMode mode;
        [Range(0, 100)] public int weight = 10;
    }

    [Header("References")]
    public Transform player;
    public GameObject projectilePrefab;
    public GameObject minionPrefab;
    public GameObject dashTargetIndicatorPrefab;

    [Header("Settings")]
    public float patternInterval = 5f;
    public float dashDuration = 0.4f;
    public float dashCooldown = 3f;
    public float knockbackForce = 7f;

    [Header("Pattern Configuration")]
    public List<BossAttackEntry> attackPatterns = new List<BossAttackEntry>();

    private Rigidbody2D rb;
    private Vector3 originalScale;
    private bool canDash = true;
    private bool isDashing = false;
    private GameObject dashIndicatorInstance;
    private Coroutine indicatorFollowRoutine;

    public AudioClip spawnSound;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;

        if (spawnSound != null)
            AudioSource.PlayClipAtPoint(spawnSound, Camera.main.transform.position);

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        StartCoroutine(BeginBossWithEntrance());
    }

    IEnumerator BeginBossWithEntrance()
    {
        yield return SpawnEntranceAnimation();
        StartCoroutine(ExecutePatternLoop());
    }

    IEnumerator SpawnEntranceAnimation()
    {
        if (player == null) yield break;

        // Pre-entry screen shake
        if (CameraShake.Instance != null)
            CameraShake.Instance.TriggerShake(1f, 1f);
        yield return new WaitForSeconds(0.5f);

        // Setup boss offscreen
        Vector3 slamTarget = transform.position;
        Vector3 spawnApex = slamTarget + new Vector3(0f, 6f, -10f);

        transform.position = spawnApex;
        transform.localScale = originalScale * 4f;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);

        // Fade in + descend
        yield return StartCoroutine(FadeAlpha(0f, 1f, 0.1f));
        yield return StartCoroutine(ScaleTo(originalScale, 0.15f));

        // Impact shake
        if (CameraShake.Instance != null)
            CameraShake.Instance.TriggerShake(0.5f, 0.5f);
    }

    IEnumerator ExecutePatternLoop()
    {
        while (true)
        {
            BossMode pattern = GetRandomWeightedPattern();
            yield return StartCoroutine(ExecutePattern(pattern));
            yield return new WaitForSeconds(patternInterval);
        }
    }

    BossMode GetRandomWeightedPattern()
    {
        int totalWeight = 0;
        foreach (var entry in attackPatterns)
        {
            totalWeight += entry.weight;
        }

        int randomValue = Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (var entry in attackPatterns)
        {
            cumulative += entry.weight;
            if (randomValue < cumulative)
                return entry.mode;
        }

        return attackPatterns[0].mode;
    }

    IEnumerator ExecutePattern(BossMode pattern)
    {
        switch (pattern)
        {
            case BossMode.Dash:
                yield return DashToPlayer();
                break;
            case BossMode.ProjectileFan:
                yield return FireRadialProjectiles();
                break;
            case BossMode.ProjectileAttack:
                yield return FireAtPlayer();
                break;
            case BossMode.ProjectileRain:
                yield return ProjectileRainAttack();
                break;
            case BossMode.MinionSpawn:
                yield return SpawnMinions();
                break;
            case BossMode.SlamAttack:
                yield return SlamAttack();
                break;
        }
    }

    IEnumerator DashToPlayer()
    {
        if (player == null || !canDash) yield break;

        canDash = false;
        isDashing = true;

        dashIndicatorInstance = Instantiate(dashTargetIndicatorPrefab, player.position, Quaternion.Euler(-90f, 0f, 0f));
        indicatorFollowRoutine = StartCoroutine(FollowPlayerUntilDashStart());
        StartCoroutine(GrowIndicator(dashIndicatorInstance.transform, 0.5f, 0.4f));

        yield return new WaitForSeconds(0.5f);

        if (indicatorFollowRoutine != null) StopCoroutine(indicatorFollowRoutine);
        Vector2 dashTarget = dashIndicatorInstance.transform.position;
        Destroy(dashIndicatorInstance);

        float dashSpeed = Vector2.Distance(rb.position, dashTarget) / dashDuration;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            rb.MovePosition(Vector2.MoveTowards(rb.position, dashTarget, dashSpeed * Time.fixedDeltaTime));
            elapsed += Time.fixedDeltaTime;

            Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.8f, LayerMask.GetMask("Player"));
            if (hit != null && hit.CompareTag("Player"))
            {
                hit.GetComponent<PlayerHealth>()?.TakeDamage(3);
                PlayerMovement pm = hit.GetComponent<PlayerMovement>();
                if (pm != null)
                {
                    Vector2 dir = (hit.transform.position - transform.position).normalized;
                    CameraShake.Instance.TriggerShake(0.2f, 0.5f);
                    pm.ApplyKnockback(dir, knockbackForce);
                }
                break;
            }
            yield return new WaitForFixedUpdate();
        }

        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    IEnumerator FireRadialProjectiles()
    {
        yield return SmoothPulseAnimation();

        int count = Random.Range(10, 36);
        for (int i = 0; i < count; i++)
        {
            float angle = i * (360f / count) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            proj.GetComponent<Rigidbody2D>().linearVelocity = dir * 4f;
        }
    }

    IEnumerator FireAtPlayer()
    {
        yield return SmoothPulseAnimation();

        int shots = Random.Range(3, 6);
        for (int i = 0; i < shots; i++)
        {
            if (player != null)
            {
                Vector2 dir = (player.position - transform.position).normalized;
                GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                proj.GetComponent<Rigidbody2D>().linearVelocity = dir * 5f;
            }
            yield return new WaitForSeconds(0.25f);
        }
    }

    IEnumerator ProjectileRainAttack()
    {
        int zoneCount = Random.Range(2, 4);

        for (int z = 0; z < zoneCount; z++)
        {
            Vector2 zonePos = player.position + (Vector3)Random.insideUnitCircle * 8f;
            GameObject indicator = Instantiate(dashTargetIndicatorPrefab, zonePos, Quaternion.Euler(-90f, 0f, 0f));
            indicator.transform.localScale = Vector3.zero;
            StartCoroutine(GrowIndicator(indicator.transform, 1f, 4f));

            yield return new WaitForSeconds(0.6f);

            int count = Random.Range(10, 50);
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Random.insideUnitCircle * 8f;
                Vector2 target = zonePos + offset;
                GameObject proj = Instantiate(projectilePrefab);
                proj.transform.position = (Vector3)target + Vector3.up * 10f + Vector3.back * 10f;
                proj.transform.localScale = Vector3.one * 3f;
                SpriteRenderer sr = proj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = new Color(1f, 1f, 1f, 0.5f);

                StartCoroutine(ProjectileFallEffect(proj, target, 0.5f));
                yield return new WaitForSeconds(0.05f);
            }

            yield return new WaitForSeconds(1f);
            Destroy(indicator);
        }
    }

    IEnumerator ProjectileFallEffect(GameObject proj, Vector2 target, float duration)
    {
        Vector3 start = proj.transform.position;
        Vector3 end = target;
        Vector3 startScale = proj.transform.localScale;
        Vector3 endScale = Vector3.one;
        SpriteRenderer sr = proj.GetComponent<SpriteRenderer>();
        Color startColor = sr != null ? sr.color : Color.white;
        Color endColor = startColor; endColor.a = 1f;

        float t = 0f;
        while (t < 1f)
        {
            proj.transform.position = Vector3.Lerp(start, end, t);
            proj.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            if (sr != null) sr.color = Color.Lerp(startColor, endColor, t);
            t += Time.deltaTime / duration;
            yield return null;
        }

        proj.transform.position = end;
        if (sr != null) sr.color = endColor;

        CameraShake.Instance.TriggerShake(0.1f, 0.1f);

        Collider2D hit = Physics2D.OverlapCircle(end, 0.6f, LayerMask.GetMask("Player"));
        if (hit != null && hit.CompareTag("Player"))
            hit.GetComponent<PlayerHealth>()?.TakeDamage(2);

        Destroy(proj);
    }

    IEnumerator SpawnMinions()
    {
        yield return SmoothPulseAnimation();

        for (int i = 0; i < 4; i++)
        {
            Vector2 offset = Random.insideUnitCircle.normalized * 2f;
            Instantiate(minionPrefab, (Vector2)transform.position + offset, Quaternion.identity);
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator SlamAttack()
    {
        if (player == null) yield break;

        // Shrink to prepare for jump
        yield return StartCoroutine(ScaleTo(originalScale * 0.8f, 0.15f));

        // Create landing indicator
        Vector3 slamTarget = player.position;
        GameObject marker = Instantiate(dashTargetIndicatorPrefab, slamTarget, Quaternion.Euler(-90f, 0f, 0f));
        StartCoroutine(GrowIndicator(marker.transform));

        // Animate jump to apex above player, scale to 4x
        Vector3 jumpApex = slamTarget + new Vector3(0f, 6f, -10f);
        Vector3 startPos = transform.position;
        float jumpTime = 0.3f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / jumpTime;
            transform.position = Vector3.Lerp(startPos, jumpApex, t);
            transform.localScale = Vector3.Lerp(originalScale * 0.8f, originalScale * 4f, t);
            yield return null;
        }

        // Fade out and hide
        yield return StartCoroutine(FadeAlpha(1f, 0f, 0.2f));
        transform.localScale = Vector3.zero;

        // Wait while indicator grows
        yield return new WaitForSeconds(0.7f);

        // Teleport above slam location
        transform.position = slamTarget;
        transform.localScale = originalScale * 4f;
        Destroy(marker);
        yield return StartCoroutine(FadeAlpha(0f, 1f, 0.1f));

        // Slam impact (scale down + shake + sfx)
        yield return StartCoroutine(ScaleTo(originalScale, 0.15f));
        if (spawnSound != null)
            AudioSource.PlayClipAtPoint(spawnSound, Camera.main.transform.position);

        if (CameraShake.Instance != null)
            CameraShake.Instance.TriggerShake(0.5f, 0.5f);

        // Deal damage + knockback
        Collider2D hit = Physics2D.OverlapCircle(transform.position, 1.5f, LayerMask.GetMask("Player"));
        if (hit != null && hit.CompareTag("Player"))
        {
            hit.GetComponent<PlayerHealth>()?.TakeDamage(4);
            PlayerMovement pm = hit.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                Vector2 dir = (hit.transform.position - transform.position).normalized;
                pm.StartCoroutine(pm.ApplyKnockback(dir, knockbackForce * 1.5f));
            }
        }
    }

    IEnumerator GrowIndicator(Transform indicator, float duration = 0.5f, float finalScale = 0.5f)
    {
        Vector3 start = Vector3.zero;
        Vector3 end = Vector3.one * finalScale;
        float t = 0f;
        while (t < 1f)
        {
            indicator.localScale = Vector3.Lerp(start, end, t);
            t += Time.deltaTime / duration;
            yield return null;
        }
        indicator.localScale = end;
    }

    IEnumerator FollowPlayerUntilDashStart()
    {
        while (dashIndicatorInstance != null && player != null)
        {
            dashIndicatorInstance.transform.position = player.position;
            yield return null;
        }
    }

    IEnumerator SmoothPulseAnimation()
    {
        Vector3 max = originalScale * 1.5f;
        Vector3 min = originalScale * 0.5f;
        float t = 0f;
        while (t < 1f)
        {
            transform.localScale = Vector3.Lerp(originalScale, max, t);
            t += Time.deltaTime * 6f;
            yield return null;
        }
        t = 0f;
        while (t < 1f)
        {
            transform.localScale = Vector3.Lerp(max, originalScale, t);
            t += Time.deltaTime * 6f;
            yield return null;
        }
    }

    IEnumerator ScaleTo(Vector3 target, float duration)
    {
        Vector3 start = transform.localScale;
        float t = 0f;
        while (t < 1f)
        {
            transform.localScale = Vector3.Lerp(start, target, t);
            t += Time.deltaTime / duration;
            yield return null;
        }
        transform.localScale = target;
    }

    IEnumerator FadeAlpha(float from, float to, float duration)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        float t = 0f;
        Color startColor = sr.color;
        while (t < 1f)
        {
            float alpha = Mathf.Lerp(from, to, t);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            t += Time.deltaTime / duration;
            yield return null;
        }
        sr.color = new Color(startColor.r, startColor.g, startColor.b, to);
    }
}