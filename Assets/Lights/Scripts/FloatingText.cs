using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float duration = 1f;

    private float lifetime;
    public RectTransform rectTransform;

    void Update()
    {
        // Monte légèrement
        rectTransform.localPosition += Vector3.up * floatSpeed * Time.deltaTime;

        // Fade-out progressif
        lifetime += Time.deltaTime / duration;

        // Destruction quand invisible
        if (lifetime >= 1f)
            Destroy(gameObject);
    }

    public void Initialize(Vector3 position)
    {
        rectTransform.localPosition = new Vector3(position.x-960f,position.y-540f);
    }
}
