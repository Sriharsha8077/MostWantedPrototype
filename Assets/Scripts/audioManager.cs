using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Master volumes")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float sirenMaxVolume = 0.35f;

    [Header("Music")]
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;
    [Tooltip("Optional. Element 0 = Wanted Level 2, 1 = Level 3, 2 = Level 4, 3 = Level 5. Empty = keep the current track.")]
    public AudioClip[] levelMusic = new AudioClip[4];
    public float musicFadeTime = 1.5f;

    [Header("Ambience")]
    public AudioClip sirenLoop;

    [Header("Game SFX")]
    public AudioClip runStartSound;
    public AudioClip wantedLevelUpSound;
    public AudioClip caughtSound;
    public AudioClip gameOverJingle;
    public AudioClip newBestSound;

    [Header("UI SFX")]
    public AudioClip uiClickSound;
    public AudioClip uiDeniedSound;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioSource sirenSource;
    private Coroutine musicRoutine;
    private bool muted;
    private const string MutePref = "audio_muted";

    void Awake()
    {
        Instance = this;
        musicSource = MakeSource("MusicSource", true);
        sfxSource = MakeSource("SfxSource", false);
        sirenSource = MakeSource("SirenSource", true);

        muted = PlayerPrefs.GetInt(MutePref, 0) == 1;
        AudioListener.volume = muted ? 0f : 1f;
    }

    void Start()
    {
        GameManager.Instance.OnRunStart += HandleRunStart;
        GameManager.Instance.OnRunEnd += HandleRunEnd;
        DifficultyManager.Instance.OnWantedLevelChanged += HandleWantedLevelChanged;

        // If the run already started before we subscribed, catch up manually.
        if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            HandleRunStart();
        }
        // String compare on purpose: this compiles whether or not your GameManager
        // has the intro-screen "WaitingToStart" state.
        else if (GameManager.Instance.CurrentState.ToString() == "WaitingToStart")
        {
            PlayMusic(menuMusic);
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRunStart -= HandleRunStart;
            GameManager.Instance.OnRunEnd -= HandleRunEnd;
        }
        if (DifficultyManager.Instance != null)
            DifficultyManager.Instance.OnWantedLevelChanged -= HandleWantedLevelChanged;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) ToggleMute();

        // Lets you drag the Music Volume slider live while playing to tune it.
        if (musicRoutine == null && musicSource.isPlaying)
            musicSource.volume = musicVolume;
    }

    AudioSource MakeSource(string objName, bool loop)
    {
        GameObject go = new GameObject(objName);
        go.transform.SetParent(transform);
        AudioSource s = go.AddComponent<AudioSource>();
        s.playOnAwake = false;
        s.loop = loop;
        s.spatialBlend = 0f; // fully 2D, no distance falloff
        return s;
    }

    // ---------- event handlers ----------

    void HandleRunStart()
    {
        PlaySFX(runStartSound);
        PlayMusic(gameplayMusic);

        if (sirenLoop != null)
        {
            sirenSource.clip = sirenLoop;
            sirenSource.volume = SirenVolume(DifficultyManager.Instance.WantedLevel);
            sirenSource.Play();
        }
    }

    void HandleRunEnd(string cause)
    {
        StopMusic();
        sirenSource.Stop();

        // The nuke already played its own big sound from AbilityManager.
        if (cause != "Nuclear Detonation") PlaySFX(caughtSound);

        StartCoroutine(PlayDelayed(gameOverJingle, 1.0f));
    }

    void HandleWantedLevelChanged(int level)
    {
        PlaySFX(wantedLevelUpSound);

        if (sirenSource.isPlaying) sirenSource.volume = SirenVolume(level);

        int idx = level - 2; // element 0 = level 2
        if (levelMusic != null && idx >= 0 && idx < levelMusic.Length && levelMusic[idx] != null)
            PlayMusic(levelMusic[idx]);
    }

    float SirenVolume(int level)
    {
        return sirenMaxVolume * sfxVolume * Mathf.Lerp(0.25f, 1f, (level - 1) / 4f);
    }

    // ---------- public API ----------

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume) * sfxVolume);
    }

    public void PlayAbility(AbilityDefinition ability)
    {
        PlaySFX(ability.activationSound, ability.soundVolume);
    }

    public void PlayUIClick() { PlaySFX(uiClickSound); }
    public void PlayUIDenied() { PlaySFX(uiDeniedSound); }
    public void PlayNewBest() { StartCoroutine(PlayDelayed(newBestSound, 2.2f)); }

    public void ToggleMute()
    {
        muted = !muted;
        AudioListener.volume = muted ? 0f : 1f;
        PlayerPrefs.SetInt(MutePref, muted ? 1 : 0);
        PlayerPrefs.Save();
    }

    // ---------- music ----------

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        if (musicRoutine != null) StopCoroutine(musicRoutine);
        musicRoutine = StartCoroutine(FadeToTrack(clip));
    }

    public void StopMusic()
    {
        if (musicRoutine != null) StopCoroutine(musicRoutine);
        musicRoutine = StartCoroutine(FadeOutAndStop());
    }

    IEnumerator FadeToTrack(AudioClip clip)
    {
        float half = Mathf.Max(0.01f, musicFadeTime * 0.5f);

        if (musicSource.isPlaying)
        {
            float startVol = musicSource.volume;
            for (float t = 0f; t < half; t += Time.unscaledDeltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVol, 0f, t / half);
                yield return null;
            }
        }

        musicSource.clip = clip;
        musicSource.volume = 0f;
        musicSource.Play();

        for (float t = 0f; t < half; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, musicVolume, t / half);
            yield return null;
        }

        musicSource.volume = musicVolume;
        musicRoutine = null;
    }

    IEnumerator FadeOutAndStop()
    {
        float half = Mathf.Max(0.01f, musicFadeTime * 0.5f);
        float startVol = musicSource.volume;

        for (float t = 0f; t < half; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVol, 0f, t / half);
            yield return null;
        }

        musicSource.Stop();
        musicRoutine = null;
    }

    IEnumerator PlayDelayed(AudioClip clip, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        PlaySFX(clip);
    }
}