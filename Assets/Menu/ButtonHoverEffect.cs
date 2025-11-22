using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TMPHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI text;
    public float hoverScale = 1.15f;
    public float glowIntensity = 1.5f;

    private Vector3 originalScale;
    private float originalGlow;

    void Start()
    {
        originalScale = text.transform.localScale;
        originalGlow = text.fontMaterial.GetFloat("_GlowPower");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.transform.localScale = originalScale * hoverScale;
        text.fontMaterial.SetFloat("_GlowPower", glowIntensity);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.transform.localScale = originalScale;
        text.fontMaterial.SetFloat("_GlowPower", originalGlow);
    }
}