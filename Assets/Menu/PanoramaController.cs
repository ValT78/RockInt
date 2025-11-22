using UnityEngine;
using UnityEngine.UI;

public class PanoramaScroll : MonoBehaviour
{
    public RawImage rawImage;
    public float scrollSpeed = 0.1f;

    void Update()
    {
        Rect uvRect = rawImage.uvRect;
        uvRect.x += scrollSpeed * Time.deltaTime;
        rawImage.uvRect = uvRect;
    }
}