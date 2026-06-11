using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    public AudioClip[] musicTracks;

    [Header("SFX Clips")]
    public AudioClip[] sfxClips;

    [Header("Default Volume")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
        musicSource.loop = true;
    }

    // ── MUSIC ──────────────────────────────────────────────

    public void PlayMusic(int index)
    {
        if (!IsValidMusic(index)) return;

        musicSource.clip = musicTracks[index];
        musicSource.Play();
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic() => musicSource.Stop();

    public void PauseMusic() => musicSource.Pause();

    public void ResumeMusic() => musicSource.UnPause();

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    // ── SFX ───────────────────────────────────────────────

    public void PlaySFX(int index)
    {
        if (!IsValidSFX(index)) return;

        sfxSource.PlayOneShot(sfxClips[index], sfxVolume);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }

    // ── MUTE ──────────────────────────────────────────────

    public void MuteMusic(bool mute) => musicSource.mute = mute;

    public void MuteSFX(bool mute) => sfxSource.mute = mute;

    public void MuteAll(bool mute)
    {
        musicSource.mute = mute;
        sfxSource.mute = mute;
    }

    public void ToggleMuteAll()
    {
        bool mute = !musicSource.mute;
        musicSource.mute = mute;
        sfxSource.mute = mute;
    }

    public bool IsMuted => musicSource.mute;
    public bool IsMusicMuted => musicSource.mute;
    public bool IsSFXMuted => sfxSource.mute;

    // ── HELPERS ───────────────────────────────────────────

    private bool IsValidMusic(int index)
    {
        if (musicTracks == null || index < 0 || index >= musicTracks.Length)
        {
            Debug.LogWarning($"AudioManager: music index {index} không hợp lệ.");
            return false;
        }
        return true;
    }

    private bool IsValidSFX(int index)
    {
        if (sfxClips == null || index < 0 || index >= sfxClips.Length)
        {
            Debug.LogWarning($"AudioManager: sfx index {index} không hợp lệ.");
            return false;
        }
        return true;
    }
}
