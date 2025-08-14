using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;
    public bool loop = false;

    [HideInInspector]
    public AudioSource source;
}

public class SoundManager : MonoBehaviour, ISaveableBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Sound Categories")]
    [SerializeField] private Sound[] uiSounds;
    [SerializeField] private Sound[] gameplaySounds;
    [SerializeField] private Sound[] musicTracks;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;

    [Header("Settings")]
    public bool soundEnabled = true;
    public bool musicEnabled = true;

    private Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();
    private AudioSource musicSource;
    private string currentMusic = "";

    // Save data
    [System.Serializable]
    public class SoundSaveData
    {
        public float masterVolume = 1f;
        public float sfxVolume = 1f;
        public float musicVolume = 1f;
        public bool soundEnabled = true;
        public bool musicEnabled = true;
    }

    private SoundSaveData saveData = new SoundSaveData();

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SaveableRegistry.Register(this);
            InitializeSounds();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Load(); // Load saved settings
        ApplyVolumeSettings();
    }

    void InitializeSounds()
    {
        // Create AudioSources for all sounds and add to dictionary
        InitializeSoundArray(uiSounds);
        InitializeSoundArray(gameplaySounds);
        InitializeSoundArray(musicTracks);

        // Create dedicated music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
    }

    void InitializeSoundArray(Sound[] sounds)
    {
        foreach (Sound sound in sounds)
        {
            if (sound.clip == null)
            {
                Debug.LogWarning($"Sound '{sound.name}' has no AudioClip assigned!");
                continue;
            }

            // Create AudioSource for this sound
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
            sound.source.playOnAwake = false;

            // Add to dictionary for quick lookup
            if (!soundDictionary.ContainsKey(sound.name))
            {
                soundDictionary.Add(sound.name, sound);
            }
            else
            {
                Debug.LogWarning($"Duplicate sound name: '{sound.name}'. Each sound must have a unique name!");
            }
        }
    }

    // ===== Play Sound Methods =====
    public void PlaySound(string soundName)
    {
        if (!soundEnabled) return;

        if (soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            if (sound.source != null)
            {
                sound.source.Play();
            }
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' not found!");
        }
    }

    public void PlaySoundOneShot(string soundName)
    {
        if (!soundEnabled) return;

        if (soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            if (sound.source != null)
            {
                sound.source.PlayOneShot(sound.clip);
            }
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' not found!");
        }
    }

    public void PlaySoundWithDelay(string soundName, float delay)
    {
        if (!soundEnabled) return;
        StartCoroutine(PlaySoundDelayed(soundName, delay));
    }

    private IEnumerator PlaySoundDelayed(string soundName, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlaySound(soundName);
    }

    public void StopSound(string soundName)
    {
        if (soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            if (sound.source != null && sound.source.isPlaying)
            {
                sound.source.Stop();
            }
        }
    }

    public void StopAllSounds()
    {
        foreach (Sound sound in soundDictionary.Values)
        {
            if (sound.source != null && sound.source.isPlaying)
            {
                sound.source.Stop();
            }
        }
    }

    // ===== Music Methods =====
    public void PlayMusic(string musicName)
    {
        if (!musicEnabled) return;

        if (soundDictionary.TryGetValue(musicName, out Sound music))
        {
            if (currentMusic == musicName && musicSource.isPlaying)
                return; // Already playing this music

            musicSource.clip = music.clip;
            musicSource.volume = music.volume * musicVolume * masterVolume;
            musicSource.pitch = music.pitch;
            musicSource.Play();
            currentMusic = musicName;

            Debug.Log($"Playing music: {musicName}");
        }
        else
        {
            Debug.LogWarning($"Music '{musicName}' not found!");
        }
    }

    public void StopMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
            currentMusic = "";
        }
    }

    public void PauseMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (!musicSource.isPlaying && musicSource.clip != null)
        {
            musicSource.UnPause();
        }
    }

    public void FadeOutMusic(float duration)
    {
        StartCoroutine(FadeMusic(musicSource.volume, 0f, duration, true));
    }

    public void FadeInMusic(string musicName, float duration)
    {
        PlayMusic(musicName);
        StartCoroutine(FadeMusic(0f, musicVolume * masterVolume, duration, false));
    }

    private IEnumerator FadeMusic(float startVolume, float targetVolume, float duration, bool stopAfterFade)
    {
        float currentTime = 0f;
        musicSource.volume = startVolume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / duration);
            yield return null;
        }

        musicSource.volume = targetVolume;

        if (stopAfterFade && targetVolume <= 0f)
        {
            StopMusic();
        }
    }

    // ===== Volume Control =====
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyVolumeSettings();
        Save(); // Auto-save
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplyVolumeSettings();
        Save(); // Auto-save
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplyVolumeSettings();
        Save(); // Auto-save
    }

    public void ToggleSound()
    {
        soundEnabled = !soundEnabled;
        ApplyVolumeSettings();
        Save(); // Auto-save
    }

    public void ToggleMusic()
    {
        musicEnabled = !musicEnabled;
        ApplyVolumeSettings();
        Save(); // Auto-save
    }

    void ApplyVolumeSettings()
    {
        // Apply to all SFX sounds
        foreach (Sound sound in soundDictionary.Values)
        {
            if (sound.source != null)
            {
                float finalVolume = soundEnabled ? sound.volume * sfxVolume * masterVolume : 0f;
                sound.source.volume = finalVolume;
            }
        }

        // Apply to music
        if (musicSource != null && !string.IsNullOrEmpty(currentMusic))
        {
            if (soundDictionary.TryGetValue(currentMusic, out Sound currentMusicSound))
            {
                float finalMusicVolume = musicEnabled ? currentMusicSound.volume * musicVolume * masterVolume : 0f;
                musicSource.volume = finalMusicVolume;
            }
        }
    }

    // ===== Utility Methods =====
    public bool IsPlaying(string soundName)
    {
        if (soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            return sound.source != null && sound.source.isPlaying;
        }
        return false;
    }

    public bool IsMusicPlaying()
    {
        return musicSource != null && musicSource.isPlaying;
    }

    public string GetCurrentMusic()
    {
        return currentMusic;
    }

    // ===== Save System Implementation =====
    public void Save()
    {
        saveData.masterVolume = masterVolume;
        saveData.sfxVolume = sfxVolume;
        saveData.musicVolume = musicVolume;
        saveData.soundEnabled = soundEnabled;
        saveData.musicEnabled = musicEnabled;

        FileSaveSystem.SaveToFile("sound_settings", saveData);
    }

    public void Load()
    {
        var loadedData = FileSaveSystem.LoadFromFile<SoundSaveData>("sound_settings");

        if (loadedData != null)
        {
            saveData = loadedData;
            masterVolume = saveData.masterVolume;
            sfxVolume = saveData.sfxVolume;
            musicVolume = saveData.musicVolume;
            soundEnabled = saveData.soundEnabled;
            musicEnabled = saveData.musicEnabled;

            Debug.Log("Sound settings loaded successfully");
        }
        else
        {
            Debug.Log("No sound settings found, using defaults");
        }
    }

    // ===== Debug Methods =====
    [ContextMenu("Test UI Click Sound")]
    void TestUIClick() => PlaySound("ui_click");

    [ContextMenu("Test Perfect Hit Sound")]
    void TestPerfectHit() => PlaySound("perfect_hit");

    [ContextMenu("List All Sounds")]
    void ListAllSounds()
    {
        Debug.Log("=== ALL REGISTERED SOUNDS ===");
        foreach (var kvp in soundDictionary)
        {
            Debug.Log($"'{kvp.Key}' -> {kvp.Value.clip?.name ?? "No Clip"}");
        }
    }

    private void OnDestroy()
    {
        SaveableRegistry.Unregister(this);
    }
}