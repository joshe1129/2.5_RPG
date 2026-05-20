using UnityEngine;
using UnityEngine.Audio;
using RPGInterfaces;
using RPG.Core;

/// <summary>
/// Singleton manager for handling audio settings and mixer control.
/// Manages master, music, UI, and SFX volume levels with persistence via PlayerPrefs.
/// Supports mute functionality and automatic volume restoration.
/// Implements IAudioManager interface for dependency injection.
/// </summary>
public class AudioManager : MonoBehaviour, IAudioManager
{
    /// Singleton instance of the AudioManager, used internally for duplicate prevention.
    private static AudioManager instance;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    // Constants moved to GameConstants
    private const float MUTE_DB = -80f;

    private float cachedMasterVolume = 0.3f;
    private float cachedMusicVolume = 0.3f;
    private float cachedUIVolume = 0.3f;
    private float cachedSFXVolume = 0.3f;

    private bool cachedMuted = false;

    /// <summary>
    /// Initializes the singleton instance and ensures persistence across scenes.
    /// Loads saved audio state from PlayerPrefs.
    /// </summary>
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        ServiceLocator.RegisterService<IAudioManager>(this);

        LoadAudioState();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            ServiceLocator.UnregisterService<IAudioManager>();
        }
    }

    /// <summary>
    /// Applies the loaded audio state to the AudioMixer after initialization.
    /// </summary>
    private void Start()
    {
        ApplyAudioState();
    }

    /// <summary>
    /// Loads audio volume and mute settings from PlayerPrefs.
    /// Restores cached values for master, music, UI, and SFX volumes, plus mute state.
    /// </summary>
    private void LoadAudioState()
    {
        cachedMasterVolume = PlayerPrefs.GetFloat(GameConstants.PREFS_MASTER_VOLUME, 0.3f);
        cachedMusicVolume = PlayerPrefs.GetFloat(GameConstants.PREFS_MUSIC_VOLUME, 0.3f);
        cachedUIVolume = PlayerPrefs.GetFloat(GameConstants.PREFS_UI_VOLUME, 0.3f);
        cachedSFXVolume = PlayerPrefs.GetFloat(GameConstants.PREFS_SFX_VOLUME, 0.3f);
        cachedMuted = PlayerPrefs.GetInt(GameConstants.PREFS_AUDIO_MUTED, 0) == 1;
    }

    /// <summary>
    /// Applies cached volume and mute settings to the AudioMixer.
    /// Converts linear volume to decibels and sets exposed parameters.
    /// </summary>
    private void ApplyAudioState()
    {
        float masterDb = cachedMuted
            ? MUTE_DB
            : ConvertLinearToDecibels(cachedMasterVolume);

        audioMixer.SetFloat("MasterVolume", masterDb);
        audioMixer.SetFloat("MusicVolume", ConvertLinearToDecibels(cachedMusicVolume));
        audioMixer.SetFloat("UIVolume", ConvertLinearToDecibels(cachedUIVolume));
        audioMixer.SetFloat("SFXVolume", ConvertLinearToDecibels(cachedSFXVolume));
    }

    /// <summary>
    /// Converts linear volume (0-1) to decibel scale for AudioMixer.
    /// Clamps input to valid range and uses logarithmic conversion.
    /// </summary>
    /// <param name="linearVolume">Linear volume value between 0 and 1.</param>
    /// <returns>Decibel value for AudioMixer parameter.</returns>
    private static float ConvertLinearToDecibels(float linearVolume)
    {
        linearVolume = Mathf.Clamp01(linearVolume);
        return Mathf.Log10(Mathf.Max(linearVolume, 0.0001f)) * 20f;
    }

    /// <summary>
    /// Sets the master volume level and persists the setting.
    /// If not muted, immediately applies the volume to the AudioMixer.
    /// </summary>
    /// <param name="volume">New master volume level (0-1).</param>
    public void SetMasterVolume(float volume)
    {
        cachedMasterVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(GameConstants.PREFS_MASTER_VOLUME, cachedMasterVolume);

        if (!cachedMuted)
        {
            float db = ConvertLinearToDecibels(cachedMasterVolume);
            audioMixer.SetFloat("MasterVolume", db);
        }
    }

    /// <summary>
    /// Sets the music volume level and persists the setting.
    /// Immediately applies the volume to the AudioMixer.
    /// </summary>
    /// <param name="volume">New music volume level (0-1).</param>
    public void SetMusicVolume(float volume)
    {
        cachedMusicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(GameConstants.PREFS_MUSIC_VOLUME, cachedMusicVolume);
        audioMixer.SetFloat("MusicVolume", ConvertLinearToDecibels(cachedMusicVolume));
    }

    /// <summary>
    /// Sets the UI volume level and persists the setting.
    /// Immediately applies the volume to the AudioMixer.
    /// </summary>
    /// <param name="volume">New UI volume level (0-1).</param>
    public void SetUIVolume(float volume)
    {
        cachedUIVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(GameConstants.PREFS_UI_VOLUME, cachedUIVolume);
        audioMixer.SetFloat("UIVolume", ConvertLinearToDecibels(cachedUIVolume));
    }

    /// <summary>
    /// Sets the SFX volume level and persists the setting.
    /// Immediately applies the volume to the AudioMixer.
    /// </summary>
    /// <param name="volume">New SFX volume level (0-1).</param>
    public void SetSFXVolume(float volume)
    {
        cachedSFXVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(GameConstants.PREFS_SFX_VOLUME, cachedSFXVolume);
        audioMixer.SetFloat("SFXVolume", ConvertLinearToDecibels(cachedSFXVolume));
    }
    
    public float GetMasterVolume() => cachedMasterVolume;
    
    public float GetMusicVolume() => cachedMusicVolume;
    
    public float GetUIVolume() => cachedUIVolume;
    
    public float GetSFXVolume() => cachedSFXVolume;

    /// <summary>
    /// Toggles the mute state for all audio.
    /// Persists the setting and applies mute by setting master volume to -80dB.
    /// </summary>
    /// <param name="mute">True to mute audio, false to unmute.</param>
    public void SetMuteState(bool mute)
    {
        cachedMuted = mute;
        PlayerPrefs.SetInt(GameConstants.PREFS_AUDIO_MUTED, mute ? 1 : 0);

        float masterDb = mute
            ? MUTE_DB
            : ConvertLinearToDecibels(cachedMasterVolume);

        audioMixer.SetFloat("MasterVolume", masterDb);
    }

    /// <summary>
    /// Gets the current mute state.
    /// </summary>
    /// <returns>True if audio is muted, false otherwise.</returns>
    public bool IsMuted() => cachedMuted;
}