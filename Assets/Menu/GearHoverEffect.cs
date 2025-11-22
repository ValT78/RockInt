using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GearButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image gearIcon;
    public Button btnClose;
    public float hoverScale = 1.3f;
    public Color hoverColor = Color.yellow;

    private Vector3 originalScale;
    private Color originalColor;
    private bool isHovering = false;

    void Start()
    {
        InitializeButton();
    }

    void InitializeButton()
    {
        originalScale = gearIcon.transform.localScale;
        originalColor = gearIcon.color;
    }

    void Update()
    {
        if (btnClose != null)
        {
            ResetButton();
        }
    }

    public void ResetButton()
    {
        isHovering = false;
        if (gearIcon != null)
        {
            gearIcon.transform.localScale = originalScale;
            gearIcon.color = originalColor;
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
        ResetButton();
    }
}