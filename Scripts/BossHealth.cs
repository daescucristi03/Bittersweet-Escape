using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class BossHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 20;
    private int currentHealth;

    [Header("UI Prefab & Name")]
    public GameObject healthBarPrefab;
    private GameObject barGO;
    public string bossName = "Cupcake Wannabe";
    public bool isFinalBoss = false;
    public AudioClip deathSound;

    private RectTransform barRect;
    private Image         fillImg;
    private TMP_Text      nameTxt;

    private static Canvas cachedCanvas;

    void Start()
    {
        currentHealth = maxHealth;
        EnsureHealthBarExists();
        UpdateFill();
    }

    public void TakeDamage(int dmg)
    {
        currentHealth = Mathf.Max(currentHealth - dmg, 0);
        UpdateFill();
        StartCoroutine(ShakeBar());

        if (currentHealth <= 0)
            Die();
    }

    void EnsureHealthBarExists()
    {
        if (barRect != null) return;
        if (healthBarPrefab == null) return;

        if (cachedCanvas == null)
            cachedCanvas = FindObjectOfType<Canvas>();
        if (cachedCanvas == null)
        {
            Debug.LogError("BossHealth: No Canvas found in scene!"); 
            return;
        }

        barGO = Instantiate(healthBarPrefab, cachedCanvas.transform);
        barGO.SetActive(true);
        barRect = barGO.GetComponent<RectTransform>();

        if (barRect.childCount > 0)
            fillImg = barRect.GetChild(0).GetComponent<Image>();

        nameTxt = barRect.GetComponentInChildren<TMP_Text>();
        if (nameTxt != null)
            nameTxt.text = bossName;

        // Intro scale-in animation
        barRect.localScale = Vector3.zero;
        StartCoroutine(ScaleBarIn());
    }

    void UpdateFill()
    {
        if (fillImg != null)
            fillImg.fillAmount = (float)currentHealth / maxHealth;
    }

    IEnumerator ScaleBarIn()
    {
        float t = 0f, dur = .35f;
        while (t < 1f)
        {
            barRect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            t += Time.deltaTime / dur;
            yield return null;
        }
        barRect.localScale = Vector3.one;
    }

    IEnumerator ShakeBar()
    {
        if (barRect == null) yield break;

        Vector3 orig = barRect.localScale;
        Vector3 big  = orig * 1.15f;
        float   dur  = 0.12f;
        float   t    = 0f;

        while (t < 1f)
        {
            barRect.localScale = Vector3.Lerp(orig, big, t);
            t += Time.deltaTime / dur;
            yield return null;
        }
        t = 0f;
        while (t < 1f)
        {
            barRect.localScale = Vector3.Lerp(big, orig, t);
            t += Time.deltaTime / dur;
            yield return null;
        }
        barRect.localScale = orig;
    }

    void Die()
    {
        Debug.Log($"{bossName} defeated!");

        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, Camera.main.transform.position);
        StopAllCoroutines();
        GetComponent<BossController>()?.StopAllCoroutines();

        Destroy(barGO);

        if (isFinalBoss)
        {
            GameManager.Instance.ShowWinScreen();
        }
        Destroy(gameObject);
    }
}