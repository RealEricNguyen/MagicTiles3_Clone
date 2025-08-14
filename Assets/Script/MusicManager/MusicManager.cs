using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Playables;

public class MusicManager : MonoBehaviour, ISaveableBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Music List (Assign in Inspector)")]
    public List<MusicData> musicList = new List<MusicData>();

    [Header("UI")]
    public Transform musicListParent; // UI container (e.g., Vertical Layout Group)
    public GameObject musicButtonPrefab; // Prefab with Text + Button

    [Header("References")]
    public GameManager gameManager;

    [Header("Save Data")]
    private GameData gameData = new GameData();

    private int selectedIndex = -1;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        CreateMusicButtons();
        Load(); // Load saved data
        int selectedMusicIndex = GetSelectedMusicIndex();
        SelectMusic(selectedMusicIndex); // Load previously selected music
    }

    void CreateMusicButtons()
    {
        foreach (Transform child in musicListParent)
            Destroy(child.gameObject);

        for (int i = 0; i < musicList.Count; i++)
        {
            int index = i;
            GameObject btnObj = Instantiate(musicButtonPrefab, musicListParent);

            // Set text
            btnObj.GetComponentInChildren<Text>().text = musicList[i].title;

            // Set click action
            btnObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                SelectMusic(index);
                SoundController.Instance.PlaySound(SoundName.Click2, false); // Play click sound
            });
        }
    }

    public void SelectMusic(int index)
    {
        if (index < 0 || index >= musicList.Count) return;

        selectedIndex = index;
        MusicData selected = musicList[index];

        Debug.Log($"Selected: {selected.title}");

        // Assign to GameManager
        gameManager.music.clip = selected.clip;
        if (selected.beatMapJson != null)
        {
            gameManager.spawner.beatMapJson = selected.beatMapJson;
        }
        UIManager.Instance.HideMusicList(selected.title);
        SetSelectedMusicIndex(index); // Save selection
    }
    // ===== ISaveableBehaviour Implementation =====
    public void Save()
    {
        FileSaveSystem.SaveToFile("game_data", gameData);
        Debug.Log($"Saved game data: Selected Music = {gameData.selectedMusicIndex}");
    }

    public void Load()
    {
        var loadedData = FileSaveSystem.LoadFromFile<GameData>("game_data");

        if (loadedData != null)
        {
            gameData = loadedData;
            Debug.Log($"Loaded game data: Selected Music = {gameData.selectedMusicIndex}");
        }
        else
        {
            Debug.Log("No save file found, using default values");
            gameData = new GameData(); // Use defaults
        }
    }
    // ===== Music Selection Management =====
    public void SetSelectedMusicIndex(int musicIndex)
    {
        gameData.selectedMusicIndex = musicIndex;
        Save(); // Save immediately so player’s choice persists
    }

    public int GetSelectedMusicIndex()
    {
        return gameData.selectedMusicIndex;
    }
}
