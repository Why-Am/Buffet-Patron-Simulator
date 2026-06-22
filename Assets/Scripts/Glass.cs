using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Glass : MonoBehaviour
{
    [SerializeField]
    private GameObject liquid;

    // 0 (empty) to 1 (full)
    private float _fillLevel;
    private bool onCooldown = false;
    private Renderer liquidRenderer;

    // Amount the position moves when scale is decreased by 1
    private const float positionToScaleRatio = 0.067f;
    private const float secondsPerMinAmountIncrement = 0.02f;
    private const float minAmountIncrement = 0.01f;

    void Start()
    {
        liquidRenderer = liquid.GetComponent<Renderer>();
        if (liquidRenderer == null)
        {
            Debug.LogError("Need Renderer");
        }
        SetLevel(0);
    }

    public void Add(Color drinkColor)
    {
        if (onCooldown || _fillLevel == 1) return;

        AdjustLevel(minAmountIncrement);
        AdjustColor(minAmountIncrement, drinkColor);
        StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(secondsPerMinAmountIncrement);
        onCooldown = false;
    }

    void AdjustLevel(float amount)
    {
        SetLevel(_fillLevel + amount);
    }

    void AdjustColor(float amount, Color color)
    {
        if (amount == 0)
        {
            liquidRenderer.material.color = color;
            return;
        }

        Color currentColor = liquidRenderer.material.color;
        liquidRenderer.material.color = Color.Lerp(currentColor, color, amount / _fillLevel);
    }

    void SetLevel(float level)
    {
        level = Mathf.Clamp01(level);
        _fillLevel = level;
        liquid.transform.localScale = new Vector3(1, level, 1);
        liquid.transform.localPosition = (1 - level) * positionToScaleRatio * Vector3.down;
    }
}
