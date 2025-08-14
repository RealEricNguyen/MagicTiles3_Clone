using UnityEngine;

[System.Serializable]
public class MusicData
{
    public string title;
    public AudioClip clip;
    public TextAsset beatMapJson; // Optional: matching beatmap
}
