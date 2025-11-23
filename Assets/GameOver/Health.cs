using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public static Health Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private RectTransform icon; // heart icon RectTransform (UI)
    [SerializeField] private RectTransform uiRoot; // parent canvas transform (used to spawn particles)
    [SerializeField] private Camera mainCamera; // optional, will fallback to Camera.main

    [Header("Health values")]
    [SerializeField] int maxHealth = 5;
    [SerializeField] int currentHealth = 5;

    [Header("Visual tuning")]
    [SerializeField] float numberTweenDuration = 0.55f;
    [SerializeField] float heartPunchScale = 1.35f;
    [SerializeField] float heartPunchTime = 0.32f;
    [SerializeField] float heartRotDeg = 8f;
    [SerializeField] Color heartFlashColor = new Color(1f, 0.7f, 0.7f, 1f);
    [SerializeField] Color textFlashColor = new Color(1f, 0.9f, 0.6f, 1f);

    [Header("Particles")]
    [SerializeField] int particleCount = 8;
    [SerializeField] Vector2 particleSpeed = new Vector2(80f, 160f);
    [SerializeField] float particleLifetime = 0.6f;
    [SerializeField] float particleSize = 18f;

    [Header("Screen shake")]
    [SerializeField] float shakeIntensity = 0.04f;
    [SerializeField] float shakeDuration = 0.12f;

    // internal
    [SerializeField] Sprite builtinSprite;
    Coroutine numberTweenCoroutine;
    Coroutine heartPulseCoroutine;
    Vector3 heartOrigScale;
    Quaternion heartOrigRot;
    Color textOrigColor;
    TMP_Text tmpTextComponent;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // fallback camera
            if (mainCamera == null) mainCamera = Camera.main;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // cache
        if (healthText == null) Debug.LogError("[Health] healthText not assigned");
        tmpTextComponent = healthText;
        textOrigColor = healthText.color;
        if (icon != null)
        {
            heartOrigScale = icon.localScale;
            heartOrigRot = icon.localRotation;
        }
        SetMaxHealth(maxHealth);
        SetHealth(currentHealth);
    }

    #region Public API
    public void SetMaxHealth(int max)
    {
        maxHealth = Mathf.Max(1, max);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthImmediate();
    }

    public void SetHealth(int hp)
    {
        hp = Mathf.Clamp(hp, 0, maxHealth);
        currentHealth = hp;
        UpdateHealthImmediate();
    }

    public void Heal(int amount)
    {
        SetHealth(currentHealth + amount);
        // small heal feedback (optional) - simple scale
        if (icon != null) StartCoroutine(QuickPulse(icon, 0.92f, 0.08f));
    }

    /// <summary>
    /// Quick pulse on a UI RectTransform: grow to targetScale then bounce back.
    /// Usage: StartCoroutine(QuickPulse(icon, 0.92f, 0.08f));
    /// </summary>
    IEnumerator QuickPulse(RectTransform rt, float targetScale = 1.15f, float totalTime = 0.2f)
    {
        if (rt == null) yield break;

        Vector3 origScale = rt.localScale;
        Quaternion origRot = rt.localRotation;

        float half = Mathf.Max(0.01f, totalTime * 0.5f);
        float t = 0f;

        // Grow phase (ease out)
        while (t < half)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / half);
            // easeOutQuad
            float eased = 1f - (1f - p) * (1f - p);
            float s = Mathf.Lerp(origScale.x, targetScale, eased);
            rt.localScale = Vector3.one * s;
            // slight rotation for feel
            float rot = Mathf.Sin(p * Mathf.PI) * (heartRotDeg * 0.3f);
            rt.localRotation = Quaternion.Euler(0f, 0f, rot);
            yield return null;
        }

        // Shrink phase (overshoot back to orig with slight bounce)
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / half);
            // back-ease (overshoot then settle)
            float eased = EaseOutBack(targetScale, origScale.x, p);
            rt.localScale = Vector3.one * eased;
            // reduce rotation
            float rot = Mathf.Sin((1f - p) * Mathf.PI) * (heartRotDeg * 0.14f);
            rt.localRotation = Quaternion.Euler(0f, 0f, rot);
            yield return null;
        }

        // restore exactly
        rt.localScale = origScale;
        rt.localRotation = origRot;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        int prev = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);

        // animate number from prev -> current
        if (numberTweenCoroutine != null) StopCoroutine(numberTweenCoroutine);
        numberTweenCoroutine = StartCoroutine(AnimateNumber(prev, currentHealth, numberTweenDuration));

        // heart pulse and flash
        if (heartPulseCoroutine != null) StopCoroutine(heartPulseCoroutine);
        heartPulseCoroutine = StartCoroutine(HeartPulseRoutine());

        // spawn particles at icon
        if (uiRoot == null && icon != null) uiRoot = icon.parent as RectTransform;
        if (uiRoot != null)
            StartCoroutine(SpawnParticles(icon != null ? (Vector2)icon.position : (Vector2)healthText.transform.position));

        // screen shake
        if (mainCamera != null)
            StartCoroutine(DoScreenShake(shakeDuration, shakeIntensity));

        // optional: play SFX (uncomment if you have SoundManager and a clip)
        // SoundManager.Instance.PlaySFX(damageClip, volume: 1f, pitch: Random.Range(0.9f, 1.05f));
    }
    #endregion

    #region Internal visuals

    void UpdateHealthImmediate()
    {
        if (tmpTextComponent != null)
            tmpTextComponent.text = currentHealth.ToString();
    }

    IEnumerator AnimateNumber(int from, int to, float duration)
    {
        float t = 0f;
        int lastShown = from;
        // apply a quick text flash color at start
        tmpTextComponent.color = textFlashColor;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            // ease out
            float eased = 1f - Mathf.Pow(1f - p, 2.0f);

            float val = Mathf.Lerp(from, to, eased);
            int show = Mathf.RoundToInt(val);
            if (show != lastShown)
            {
                lastShown = show;
                tmpTextComponent.text = show.ToString();
            }

            // subtle pop scale on the text using its RectTransform
            RectTransform rt = tmpTextComponent.rectTransform;
            float scale = Mathf.Lerp(1.15f, 1f, eased);
            rt.localScale = Vector3.one * scale;

            yield return null;
        }

        tmpTextComponent.text = to.ToString();
        // restore color & scale
        tmpTextComponent.color = textOrigColor;
        tmpTextComponent.rectTransform.localScale = Vector3.one;
        numberTweenCoroutine = null;
    }

    IEnumerator HeartPulseRoutine()
    {
        if (icon == null)
            yield break;

        float t = 0f;
        float half = heartPunchTime * 0.6f;
        // set flash color if Image exists
        Image heartImg = icon.GetComponent<Image>();
        Color origHeartCol = heartImg != null ? heartImg.color : Color.white;

        // quick punch out (grow) then bounce back
        while (t < heartPunchTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / heartPunchTime);
            // out-back style easing
            float s = EaseOutBack(1f, heartPunchScale, p);
            float rot = Mathf.Sin(p * Mathf.PI) * heartRotDeg * (1f - p);

            icon.localScale = heartOrigScale * s;
            icon.localRotation = Quaternion.Euler(0f, 0f, rot);

            // flash lerp to red-ish at early phase
            if (heartImg != null)
            {
                float flashT = Mathf.Clamp01(p * 2f);
                heartImg.color = Color.Lerp(origHeartCol, heartFlashColor, flashT);
            }

            yield return null;
        }

        // restore
        icon.localScale = heartOrigScale;
        icon.localRotation = heartOrigRot;
        if (heartImg != null) heartImg.color = origHeartCol;
        heartPulseCoroutine = null;
    }

    IEnumerator SpawnParticles(Vector2 screenPos)
    {
        // spawn multiple small UI Images (use builtin UISprite)
        for (int i = 0; i < particleCount; i++)
        {
            GameObject g = new GameObject("hp_particle");
            g.transform.SetParent(uiRoot, false);
            var rt = g.AddComponent<RectTransform>();
            rt.sizeDelta = Vector2.one * particleSize;
            // position in screen space: keep using screen coordinates (icon.position is in world/screen already)
            rt.position = screenPos;

            var img = g.AddComponent<Image>();
            img.sprite = builtinSprite;
            img.color = new Color(1f, 0.6f, 0.6f, 1f); // pink-ish heart look

            // random direction
            Vector2 dir = Random.insideUnitCircle.normalized;
            float sp = Random.Range(particleSpeed.x, particleSpeed.y);

            StartCoroutine(AnimateParticle(g, rt, dir, sp));
        }
        yield return null;
    }

    IEnumerator AnimateParticle(GameObject g, RectTransform rt, Vector2 dir, float speed)
    {
        float life = 0f;
        Vector2 start = rt.anchoredPosition;
        Vector2 targetOffset = dir * (speed * 0.01f); // scale down for UI coords
        Image im = g.GetComponent<Image>();
        while (life < particleLifetime)
        {
            life += Time.deltaTime;
            float p = life / particleLifetime;
            // ease out movement
            float ease = 1f - Mathf.Pow(1f - p, 2f);
            rt.anchoredPosition = Vector2.Lerp(start, start + targetOffset, ease);
            // fade & shrink
            float a = 1f - p;
            im.color = new Color(im.color.r, im.color.g, im.color.b, a);
            rt.localScale = Vector3.one * Mathf.Lerp(1f, 0.4f, p);
            yield return null;
        }
        Destroy(g);
    }

    IEnumerator DoScreenShake(float dur, float intensity)
    {
        if (mainCamera == null) yield break;
        Vector3 orig = mainCamera.transform.localPosition;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float damper = 1f - (t / dur);
            mainCamera.transform.localPosition = orig + Random.insideUnitSphere * intensity * damper;
            yield return null;
        }
        mainCamera.transform.localPosition = orig;
    }

    #endregion

    #region Utility & easing
    // small "back" ease to get a snappy overshoot feel
    float EaseOutBack(float a, float b, float t)
    {
        // returns LERP from a to b with overshoot
        float s = 1.70158f;
        t = t - 1f;
        float res = (b - a) * (t * t * ((s + 1f) * t + s) + 1f) + a;
        return res;
    }
    #endregion
}
