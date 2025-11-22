using UnityEngine;

public class LineScript : MonoBehaviour
{
    public RectTransform targetUI;

    void Update()
    {
        targetUI.SetAsLastSibling();
    }
}