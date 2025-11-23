using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [Header("Movement")]
    public float floatSpeed = 50f;
    public float horizontalWiggle = 6f;
    public AnimationCurve riseCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Visual FX")]
    public float maxScale = 1.4f;
    public float minScale = 1f;
    public float rotationAmplitude = 15f; // degrees
    public float glowPulse = 0.4f;

    [Header("Timing")]
    public float duration = 1.2f;

    private float t = 0f;
    private Vector3 initialPosition;
    private TMP_Text tmp;
    private RectTransform rectTransform;

    void Awake()
    {
        tmp = GetComponent<TMP_Text>();
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        t += Time.deltaTime / duration;

        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // --- Movement (vertical rise + subtle wiggle)
        float rise = riseCurve.Evaluate(t) * floatSpeed;
        float wiggle = Mathf.Sin(t * Mathf.PI * 4f) * horizontalWiggle * (1f - t);
        rectTransform.localPosition = initialPosition + new Vector3(wiggle, rise, 0);

        // --- Scale punch (big at start → normal)
        float scale = Mathf.Lerp(maxScale, minScale, t);
        rectTransform.localScale = Vector3.one * scale;

        // --- Rotation sway (oscillation qui diminue)
        float rot = Mathf.Sin(t * Mathf.PI * 6f) * rotationAmplitude * (1f - t);
        rectTransform.localRotation = Quaternion.Euler(0, 0, rot);

        // --- Fade out
        Color c = tmp.color;
        c.a = 1f - t;
        tmp.color = c;

        // --- Glow pulse (outline)
        tmp.outlineWidth = glowPulse * (1f - t);
        tmp.outlineColor = new Color(1f, 1f, 1f, (1f - t) * 0.8f);
    }

    public void Initialize(Vector3 screenPosition)
    {
        // Convertit la position normalisée que tu passes
        // et l’applique en local dans l’espace UI
        initialPosition = new Vector3(
            screenPosition.x - 960f,
            screenPosition.y - 540f,
            0
        );

        rectTransform.localPosition = initialPosition;
    }
}
