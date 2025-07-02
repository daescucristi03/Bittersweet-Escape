using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    public Image healthBarFill;
    public Image healthBarBackFill;
    public float lerpSpeed = 2f;

    private float currentFill = 1f;
    private float targetFill = 1f;

    public void SetHealth(float healthPercent)
    {
        targetFill = Mathf.Clamp01(healthPercent);
        healthBarFill.fillAmount = targetFill;
    }

    void Update()
    {
        if (healthBarBackFill.fillAmount > targetFill)
        {
            healthBarBackFill.fillAmount = Mathf.MoveTowards(
                healthBarBackFill.fillAmount,
                targetFill,
                lerpSpeed * Time.deltaTime
            );
        }
        else
        {
            healthBarBackFill.fillAmount = targetFill;
        }
    }
}