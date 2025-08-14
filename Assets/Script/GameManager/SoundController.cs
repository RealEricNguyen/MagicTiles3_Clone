using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class SoundController : MonoBehaviour
{
    public static SoundController Instance { get; private set; }

    [SerializeField] private List<Sounds> sounds;
    [SerializeField] private List<Sounds> musics;

    public float SoundVolume;
    public float MusicVolume;

    public Slider soundSlider;
    public Slider musicSlider;

    private void Awake()
    {
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        for (int i = 0; i < sounds.Count; i++)
        {
            sounds[i].audio = gameObject.AddComponent<AudioSource>();
            sounds[i].audio.clip = sounds[i].clip;
            sounds[i].audio.priority = 1;
            sounds[i].audio.playOnAwake = false;
            sounds[i].audio.volume = sounds[i].volume;
        }

        for (int i = 0; i < musics.Count; i++)
        {
            musics[i].audio = gameObject.AddComponent<AudioSource>();
            musics[i].audio.clip = musics[i].clip;
            musics[i].audio.playOnAwake = false;
            musics[i].audio.volume = musics[i].volume;
        }
    }
    private void Start()
    {
        //GameHelper.Instance.dataManager.OnDataLoadComplete += LoadAudio;
    }
    public void OnSoundSliderChanged()
    {
        if (soundSlider != null)
        {
            SoundVolume = soundSlider.value;
        }
        SoundVolumeUpdate();
        SaveAudio();
    }
    public void OnMusicSliderChanged()
    {
        if (musicSlider != null)
        {
            MusicVolume = musicSlider.value;
        }
        MusicVolumeUpdate();
        SaveAudio();
    }
    public void PlaySound(SoundName name, bool loop)
    {
        Sounds sound = sounds.Find(x => x.name == name);
        sound.audio.volume = SoundVolume;
        sound.audio.loop = loop;
        sound?.audio.Play();
    }

    public void PlayMusic(SoundName name, bool loop = false)
    {
        if (!IsMusic())
        {
            return;
        }

        Sounds music = musics.Find(x => x.name == name);
        if (music != null)
        {
            music.audio.volume = MusicVolume;
            music.audio.priority = 128;
            music.audio.loop = loop;
            music.audio.Play();
        }
    }

    public void StopSound(SoundName name)
    {
        Sounds sound = sounds.Find(x => x.name == name);
        sound?.audio.Stop();
    }

    public void StopMusic(SoundName name)
    {
        Sounds music = musics.Find(x => x.name == name);
        music?.audio.Stop();
    }

    public void SoundUpdate(bool mute)
    {
        for (int i = 0; i < sounds.Count; i++)
        {
            sounds[i].audio.mute = mute;
        }
    }

    public void MusicUpdate(bool mute)
    {
        for (int i = 0; i < musics.Count; i++)
        {
            musics[i].audio.mute = mute;
        }
    }

    public bool IsSound()
    {
        return true;
    }

    public bool IsMusic()
    {
        return true;
    }

    public void MusicVolumeUpdate()
    {
        for (int i = 0; i < musics.Count; i++)
        {
            musics[i].audio.volume = MusicVolume;

        }
    }
    public void SoundVolumeUpdate()
    {
        for (int i = 0; i < sounds.Count; i++)
        {
            sounds[i].audio.volume = SoundVolume;

        }
    }
    public void SaveAudio()
    {
        //GameHelper.Instance.dataManager.SaveDataPersistent();
    }
    public void LoadAudio()
    {
        SoundVolumeUpdate();
        MusicVolumeUpdate();
        if (soundSlider != null && musicSlider != null)
        {
            soundSlider.value = SoundVolume;
            musicSlider.value = MusicVolume;
        }
    }
}

[System.Serializable]
public class Sounds
{
    public SoundName name;
    [Range(0, 1)]
    public float volume = 1;
    public AudioClip clip;
    [HideInInspector] public AudioSource audio;
}

public enum SoundName
{
    Playsound,
    Click,
    BackgroundMusic,
    Win,
    Lose,
    Click2,
}
