using UnityEngine;

public class LineForeground : MonoBehaviour
{
    public RectTransform targetUI;

    void Update()
    {
        targetUI.SetAsLastSibling();
    }
}
