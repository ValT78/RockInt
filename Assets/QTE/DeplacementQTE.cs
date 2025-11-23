using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DeplacementQTE : MonoBehaviour
{
    [Header("Movement / Timing")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private RectTransform rectTransform;

    [Header("Feedback (tweak)")]
    [SerializeField] Color successFlashColor = new Color(1f, 0.95f, 0.6f, 0.9f); // warm flash
    [SerializeField] float feedbackDuration = 0.6f;
    [SerializeField] float punchScale = 1.35f;
    [SerializeField] float punchRotationDeg = 8f;
    [SerializeField] Vector2 floatingTextOffset = new Vector2(0, 40f);
    [SerializeField] float floatingTextRise = 40f;
    [SerializeField] Color floatingTextColor = Color.white;
    [SerializeField] float screenShakeIntensity = 0.06f;
    [SerializeField] float screenShakeDuration = 0.12f;

    [Header("References")]
    public InputActionReference QTETrigger;
    public AudioClip cymbal;
    public GameObject floatingTextPrefab;
    public Transform parentCanvas;
    private FollowerController follower;
    private GameObject player;

    // internal
    bool ReadStopHeld()
    {
        if (QTETrigger != null && QTETrigger.action != null)
        {
            return QTETrigger.action.ReadValue<float>() > 0.1f;
        }
        return false;
    }

    private void Awake()
    {
        follower = FindAnyObjectByType<FollowerController>();
        parentCanvas = GameObject.Find("MainCanvas").transform;
        player = FindAnyObjectByType<LeaderController>().gameObject;
    }

    private void Start()
    {
        // sanity: reduce master music to ensure SFX audible (adjust to your SoundManager scale)
        SoundManager.Instance.SetMusicVolume(0.8f);
    }

    private void FixedUpdate()
    {
        rectTransform.localPosition += speed * Time.deltaTime * new Vector3(0, -1, 0);

        if (rectTransform.localPosition.y <= -600)
        {
            QTEManager.Instance.SpawnTouch();
            Destroy(gameObject);
            return;
        }

        if (Mathf.Abs(rectTransform.localPosition.y) <= 50 && ReadStopHeld() && follower != null && follower.CurrentState == FollowerController.State.Solidaire)
        {
            // award score immediately
            GameManager.Instance.AddScore(5);

            // trigger checkmark UI (your existing logic)
            QTEManager.Instance.Checkmark(rectTransform.localPosition.y);

            // play SFX with slight pitch variance
            SoundManager.Instance.PlaySFX(cymbal, volume: 1f, pitch: Random.Range(0.9f, 1.15f));

            // launch visual feedback coroutine, then destroy this object at the end
            StartCoroutine(PlaySuccessFeedbackAndDestroy());

            // disable further interaction immediately (makes sure we don't trigger twice)
            enabled = false;
        }
    }

    IEnumerator PlaySuccessFeedbackAndDestroy()
    {
        // 1) Punch the rectTransform (scale + rotate) while also spawn a flash image and floating text
        // store original transforms
        Vector3 origScale = rectTransform.localScale;
        Quaternion origRot = rectTransform.localRotation;

        // create flash overlay image (a sibling under same parent)
        GameObject flashGO = new GameObject("QTE_Flash");
        flashGO.transform.SetParent(rectTransform.parent, false);
        var flashRect = flashGO.AddComponent<RectTransform>();
        flashRect.sizeDelta = rectTransform.sizeDelta * 1.6f;
        flashRect.position = rectTransform.position;
        var flashImg = flashGO.AddComponent<Image>();
        flashImg.raycastTarget = false;
        flashImg.color = successFlashColor;

        // create floating "+5" text
        Instantiate(floatingTextPrefab, parentCanvas).GetComponent<FloatingText>().Initialize(Camera.main.WorldToScreenPoint(player.transform.position));

        // screen shake: capture camera
        Transform cam = Camera.main != null ? Camera.main.transform : null;
        Vector3 camOrig = cam != null ? cam.localPosition : Vector3.zero;

        float t = 0f;
        // punch animation curve: up then down quickly
        while (t < feedbackDuration)
        {
            float normalized = t / feedbackDuration;

            // scale punch (ease out)
            float scaleFactor = Mathf.Lerp(punchScale, 1f, Mathf.SmoothStep(0f, 1f, normalized));
            rectTransform.localScale = origScale * scaleFactor;

            // rotation oscillation diminishing
            float rotAmount = Mathf.Sin(normalized * Mathf.PI) * (punchRotationDeg * (1f - normalized));
            rectTransform.localRotation = origRot * Quaternion.Euler(0, 0, rotAmount);

            // flash fade out (alpha)
            float flashAlpha = Mathf.Lerp(successFlashColor.a, 0f, normalized);
            Color c = successFlashColor;
            c.a = flashAlpha;
            if (flashImg != null) flashImg.color = c;

            // small screen shake early in animation
            if (cam != null)
            {
                float shakeFactor = Mathf.Clamp01(1f - (t / screenShakeDuration));
                Vector3 shake = Random.insideUnitSphere * screenShakeIntensity * shakeFactor;
                cam.localPosition = camOrig + shake;
            }

            t += Time.deltaTime;
            yield return null;
        }

        // restore camera
        if (cam != null) cam.localPosition = camOrig;
        // cleanup UI clones
        if (flashGO != null) Destroy(flashGO);

        // restore rect transform (safety)
        rectTransform.localScale = origScale;
        rectTransform.localRotation = origRot;
    }
}
