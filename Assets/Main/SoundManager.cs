using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SoundManager : MonoBehaviour
{
    // --- Singleton ---
    static SoundManager _instance;
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find one in scene
                _instance = FindObjectOfType<SoundManager>();
                if (_instance == null)
                {
                    // Create a GameObject automatically if none present
                    GameObject go = new GameObject("[SoundManager]");
                    _instance = go.AddComponent<SoundManager>();
                }
                _instance.InitIfNeeded();
            }
            return _instance;
        }
    }

    [Header("Music")]
    [SerializeField] AudioSource musicSource;        // single music audio source (looping)
    [SerializeField] bool musicLoop = true;
    [SerializeField] float defaultMusicVolume = 0.8f;
    [SerializeField] float musicCrossfadeTime = 0.35f;

    [Header("SFX / Pool")]
    [SerializeField] int sfxPoolSize = 12;
    [SerializeField] float defaultSfxVolume = 1f;
    [SerializeField] GameObject sfxContainerPrefab; // optional prefab (not required)

    // internal pool
    List<AudioSource> sfxPool;
    Transform sfxParent;

    // runtime
    Coroutine musicFadeCoroutine;

    void Awake()
    {
        // singleton enforcement
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitIfNeeded();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void InitIfNeeded()
    {
        // create music source if missing
        if (musicSource == null)
        {
            GameObject mus = new GameObject("MusicSource");
            mus.transform.SetParent(transform);
            musicSource = mus.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = musicLoop;
            musicSource.volume = defaultMusicVolume;
        }

        // sfx parent
        if (sfxParent == null)
        {
            GameObject p = new GameObject("SFX Pool");
            p.transform.SetParent(transform);
            sfxParent = p.transform;
        }

        // create pool
        if (sfxPool == null)
        {
            sfxPool = new List<AudioSource>(sfxPoolSize);
            for (int i = 0; i < sfxPoolSize; i++)
            {
                var go = new GameObject($"SFX_{i}");
                go.transform.SetParent(sfxParent);
                var a = go.AddComponent<AudioSource>();
                a.playOnAwake = false;
                a.spatialBlend = 0f; // 2D by default
                a.volume = defaultSfxVolume;
                sfxPool.Add(a);
            }
        }
    }

    // === MUSIC API ===

    /// <summary>
    /// Plays a music clip. Stops previous clip (with optional crossfade).
    /// If crossfadeTime <= 0 => immediate switch.
    /// </summary>
    public void PlayMusic(AudioClip clip, float volume = -1f, float crossfadeTime = -1f, bool loop = true)
    {
        if (clip == null) return;
        InitIfNeeded();

        if (volume < 0f) volume = defaultMusicVolume;
        if (crossfadeTime < 0f) crossfadeTime = musicCrossfadeTime;

        // immediate swap
        if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
        if (crossfadeTime <= 0f || !musicSource.isPlaying)
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.volume = volume;
            musicSource.Play();
        }
        else
        {
            musicFadeCoroutine = StartCoroutine(CrossfadeMusic(clip, volume, crossfadeTime, loop));
        }
    }

    /// <summary>
    /// Stops music (optionally fade out).
    /// </summary>
    public void StopMusic(float fadeOutTime = 0.15f)
    {
        if (musicSource == null) return;
        if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
        if (fadeOutTime <= 0f)
        {
            musicSource.Stop();
        }
        else
        {
            musicFadeCoroutine = StartCoroutine(FadeOutMusic(fadeOutTime));
        }
    }

    IEnumerator CrossfadeMusic(AudioClip newClip, float targetVolume, float fadeTime, bool loop)
    {
        AudioSource old = musicSource;
        // create temporary AudioSource to play new clip underneath / over
        GameObject tempGO = new GameObject("MusicTemp");
        tempGO.transform.SetParent(transform);
        var tempSrc = tempGO.AddComponent<AudioSource>();
        tempSrc.clip = newClip;
        tempSrc.loop = loop;
        tempSrc.playOnAwake = false;
        tempSrc.volume = 0f;
        tempSrc.Play();

        float t = 0f;
        float oldStartVol = old.volume;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / fadeTime);
            old.volume = Mathf.Lerp(oldStartVol, 0f, p);
            tempSrc.volume = Mathf.Lerp(0f, targetVolume, p);
            yield return null;
        }

        // replace main music source clip
        old.Stop();
        old.clip = newClip;
        old.loop = loop;
        old.volume = targetVolume;
        old.Play();

        Destroy(tempGO);
        musicFadeCoroutine = null;
    }

    IEnumerator FadeOutMusic(float fadeTime)
    {
        float start = musicSource.volume;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(start, 0f, t / fadeTime);
            yield return null;
        }
        musicSource.Stop();
        musicFadeCoroutine = null;
    }

    /// <summary>
    /// Set music volume (0..1)
    /// </summary>
    public void SetMusicVolume(float v)
    {
        if (musicSource == null) return;
        musicSource.volume = Mathf.Clamp01(v);
    }

    // === SFX API ===

    /// <summary>
    /// Play a single AudioClip on an available pool source. You can vary pitch & volume.
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f, bool spatial = false, Vector3? worldPos = null)
    {
        if (clip == null) return;
        InitIfNeeded();
        var src = GetFreeSfxSource();
        if (src == null) return;

        src.clip = clip;
        src.volume = Mathf.Clamp01(volume);
        src.pitch = Mathf.Clamp(pitch, -3f, 3f);
        src.spatialBlend = spatial ? 1f : 0f;
        if (worldPos.HasValue)
        {
            src.transform.position = worldPos.Value;
        }
        src.Play();
    }

    /// <summary>
    /// Play a random clip from an array, with optional pitch and volume randomization.
    /// pitchRange: (min,max). volumeRange: (min,max)
    /// </summary>
    public void PlaySFXRandom(AudioClip[] clips, Vector2? pitchRange = null, Vector2? volumeRange = null, bool spatial = false, Vector3? worldPos = null)
    {
        if (clips == null || clips.Length == 0) return;
        var clip = clips[Random.Range(0, clips.Length)];
        float pitch = pitchRange.HasValue ? Random.Range(pitchRange.Value.x, pitchRange.Value.y) : 1f;
        float volume = volumeRange.HasValue ? Random.Range(volumeRange.Value.x, volumeRange.Value.y) : 1f;
        PlaySFX(clip, volume, pitch, spatial, worldPos);
    }

    /// <summary>
    /// Convenience: play several variations by changing pitch slightly (good for footsteps/hits).
    /// count - number of quick variations (will use pool sources sequentially).
    /// </summary>
    public void PlayVariation(AudioClip clip, int count = 1, float pitchJitter = 0.05f, float volumeJitter = 0.05f)
    {
        for (int i = 0; i < count; i++)
        {
            float p = 1f + Random.Range(-pitchJitter, pitchJitter);
            float v = Mathf.Clamp01(1f + Random.Range(-volumeJitter, volumeJitter));
            PlaySFX(clip, v, p);
        }
    }

    AudioSource GetFreeSfxSource()
    {
        // find an audio source not playing
        for (int i = 0; i < sfxPool.Count; i++)
        {
            if (!sfxPool[i].isPlaying)
                return sfxPool[i];
        }

        // none free: steal the oldest one (or expand pool if desired)
        // we choose to reuse the first
        return sfxPool.Count > 0 ? sfxPool[0] : null;
    }

    /// <summary>
    /// Adjust global SFX volume (scale of existing pool sources).
    /// </summary>
    public void SetSFXVolume(float v)
    {
        defaultSfxVolume = Mathf.Clamp01(v);
        if (sfxPool != null)
        {
            foreach (var a in sfxPool) if (a != null) a.volume = defaultSfxVolume;
        }
    }

    // Optional: stop all SFX currently playing
    public void StopAllSFX()
    {
        if (sfxPool == null) return;
        foreach (var a in sfxPool) if (a != null) a.Stop();
    }
}
