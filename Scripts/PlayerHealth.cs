using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 8;
    public int currentHealth;

    [Header("UI")]
    public Image         healthBarFill;
    public RectTransform healthBarParent;
    public Color damageFlashColor = Color.red;
    public float flashDuration     = 0.12f;

    void Awake()
    {
        if (healthBarFill == null && healthBarParent != null && healthBarParent.childCount > 0)
            healthBarFill = healthBarParent.GetChild(0).GetComponent<Image>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(currentHealth - amount, 0);
        UpdateHealthBar();
        StartCoroutine(FlashDamage());

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthBar();
    }

    public void SetMaxHealth(int newMax)
    {
        maxHealth     = Mathf.Max(1, newMax);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (healthBarFill != null)
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;

        if (healthBarParent != null)
            StartCoroutine(ShakeHealthbar());
    }

    IEnumerator ShakeHealthbar()
    {
        Vector3 original = healthBarParent.localScale;
        Vector3 enlarged = original * 1.15f;
        const float dur  = 0.10f;

        float t = 0f;
        while (t < 1f)
        {
            healthBarParent.localScale = Vector3.Lerp(original, enlarged, t);
            t += Time.deltaTime / dur;
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            healthBarParent.localScale = Vector3.Lerp(enlarged, original, t);
            t += Time.deltaTime / dur;
            yield return null;
        }

        healthBarParent.localScale = original;
    }

    IEnumerator FlashDamage()
    {
        if (healthBarFill == null) yield break;

        Color original = healthBarFill.color;
        float t = 0f;

        while (t < 1f)
        {
            healthBarFill.color = Color.Lerp(original, damageFlashColor, t);
            t += Time.deltaTime / flashDuration;
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            healthBarFill.color = Color.Lerp(damageFlashColor, original, t);
            t += Time.deltaTime / flashDuration;
            yield return null;
        }

        healthBarFill.color = original;
    }

    void Die()
    {
        Debug.Log("Player Died!");
        GameManager.Instance.ShowDeathScreen();
    }
}