using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GearButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image gearIcon;
    public float hoverScale = 1.3f;
    public Color hoverColor = Color.yellow;
    public float rotationSpeed = 90f; // Bonus : rotation au survol !

    private Vector3 originalScale;
    private Color originalColor;
    private bool isHovering = false;

    void Start()
    {
        originalScale = gearIcon.transform.localScale;
        originalColor = gearIcon.color;
    }

    void Update()
    {
        // Rotation continue pendant le survol (optionnel)
        if (isHovering)
        {
            gearIcon.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        gearIcon.transform.localScale = originalScale * hoverScale;
        gearIcon.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        gearIcon.transform.localScale = originalScale;
        gearIcon.color = originalColor;
        // Réinitialiser la rotation
        gearIcon.transform.rotation = Quaternion.identity;
    }
}