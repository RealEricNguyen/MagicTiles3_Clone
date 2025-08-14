using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Note
{
    public float time; // when the tile should be hit (in seconds, using AudioSource.time)
    public int lane;   // which lane (0..N-1)
}

[System.Serializable]
public class BeatMap
{
    public List<Note> notes;
}

[System.Serializable]
public class DifficultyLevel
{
    public string name;
    public float musicSpeed = 1f;      // AudioSource pitch/speed multiplier
    public float spawnSpeedMultiplier = 1f;  // How fast tiles fall
    public float timingWindowMultiplier = 1f; // Perfect/Good timing windows
    [Range(0.5f, 2f)]
    public float gameplaySpeed = 1f;   // Overall game speed
}

public class BeatMapSpawner : MonoBehaviour
{
    [Header("References")]
    public AudioSource music;
    public RectTransform[] lanes;
    public GameObject tilePrefab;
    public TextAsset beatMapJson;
    public TilePool tilePool;
    public RectTransform hitLine;
    public RectTransform bottomLine;

    [Header("Base Timing Settings")]
    public float baseTileFallTime = 1.5f;

    [Header("Difficulty System")]
    public DifficultyLevel[] difficultyLevels = new DifficultyLevel[]
    {
        new DifficultyLevel { name = "Easy", musicSpeed = 0.8f, spawnSpeedMultiplier = 0.8f, timingWindowMultiplier = 1.3f, gameplaySpeed = 0.8f },
        new DifficultyLevel { name = "Normal", musicSpeed = 1f, spawnSpeedMultiplier = 1f, timingWindowMultiplier = 1f, gameplaySpeed = 1f },
        new DifficultyLevel { name = "Hard", musicSpeed = 1.2f, spawnSpeedMultiplier = 1.3f, timingWindowMultiplier = 0.8f, gameplaySpeed = 1.2f },
        new DifficultyLevel { name = "Expert", musicSpeed = 1.5f, spawnSpeedMultiplier = 1.6f, timingWindowMultiplier = 0.6f, gameplaySpeed = 1.4f }
    };

    [Header("Current Settings")]
    public int currentDifficultyIndex = 1; // Normal by default

    [Header("Early Note Removal")]
    public bool removeEarlyNotes = true;
    public bool debugEarlyNotes = true;

    private BeatMap beatMap;
    private int noteIndex;
    private bool prepared = false;
    private DifficultyLevel currentDifficulty;
    private float effectiveTileFallTime; // Calculated based on difficulty

    // Pause/Resume state
    private bool isPaused = false;
    private float pauseSongTime = 0f;

    // Dynamic difficulty events
    public System.Action<DifficultyLevel> OnDifficultyChanged;

    void Start()
    {
        SetDifficulty(currentDifficultyIndex);
        LoadBeatMap();
    }

    void LoadBeatMap()
    {
        if (beatMapJson != null)
            beatMap = JsonUtility.FromJson<BeatMap>(beatMapJson.text);
        else
            beatMap = new BeatMap() { notes = new List<Note>() };

        // Remove problematic early notes based on current difficulty
        if (removeEarlyNotes && beatMap.notes != null)
        {
            RemoveEarlyNotes();
        }

        noteIndex = 0;
    }

    public void SetDifficulty(int difficultyIndex)
    {
        if (difficultyIndex < 0 || difficultyIndex >= difficultyLevels.Length)
        {
            Debug.LogWarning($"Invalid difficulty index: {difficultyIndex}");
            return;
        }

        currentDifficultyIndex = difficultyIndex;
        currentDifficulty = difficultyLevels[difficultyIndex];

        // Calculate effective fall time based on difficulty
        effectiveTileFallTime = baseTileFallTime / currentDifficulty.spawnSpeedMultiplier;

        // Apply music speed
        if (music != null)
        {
            music.pitch = currentDifficulty.musicSpeed;
        }

        // Notify other systems about difficulty change
        OnDifficultyChanged?.Invoke(currentDifficulty);

        Debug.Log($"Difficulty set to: {currentDifficulty.name} (Music: {currentDifficulty.musicSpeed}x, Spawn: {currentDifficulty.spawnSpeedMultiplier}x)");

        // Re-clean beatmap with new timing if already loaded
        if (beatMap?.notes != null && removeEarlyNotes)
        {
            RemoveEarlyNotes();
        }
    }

    void RemoveEarlyNotes()
    {
        int originalCount = beatMap.notes.Count;

        // Use effective fall time for removal calculation
        float threshold = effectiveTileFallTime / currentDifficulty.musicSpeed; // Account for music speed
        beatMap.notes.RemoveAll(note => note.time <= threshold);

        int removedCount = originalCount - beatMap.notes.Count;

        if (debugEarlyNotes && removedCount > 0)
        {
            Debug.Log($"Removed {removedCount} early notes (threshold: {threshold:F2}s) for {currentDifficulty.name} difficulty.");
        }
    }

    public void ResetAndPrepare()
    {
        noteIndex = 0;
        prepared = true;
        isPaused = false;

        // Apply current difficulty settings
        if (music != null)
        {
            music.pitch = currentDifficulty.musicSpeed;
        }

        // Optional: Clean again in case difficulty changed
        if (removeEarlyNotes)
        {
            RemoveEarlyNotes();
        }
    }

    void Update()
    {
        if (!prepared || isPaused || beatMap == null || beatMap.notes == null || beatMap.notes.Count == 0)
            return;

        if (noteIndex >= beatMap.notes.Count) return;

        // Use scaled time for spawning calculations
        float songTime = music.time;

        while (noteIndex < beatMap.notes.Count &&
               songTime >= beatMap.notes[noteIndex].time - effectiveTileFallTime)
        {
            SpawnTile(beatMap.notes[noteIndex].lane, beatMap.notes[noteIndex].time);
            noteIndex++;
        }
    }

    void SpawnTile(int laneIndex, float targetTime)
    {
        if (laneIndex < 0 || laneIndex >= lanes.Length)
        {
            Debug.LogWarning($"Invalid lane index: {laneIndex}");
            return;
        }

        TileUI tileUI = tilePool.GetTile();
        tileUI.transform.SetParent(lanes[laneIndex], false);

        RectTransform rt = tileUI.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;

        // Calculate speed based on difficulty
        float distance = Mathf.Abs(rt.position.y - hitLine.position.y);
        float speed = distance / effectiveTileFallTime;

        // Apply difficulty-based speed multiplier
        speed *= currentDifficulty.spawnSpeedMultiplier;

        tileUI.Init(targetTime, speed, tilePool, music, hitLine, bottomLine, currentDifficulty);
    }

    // ===== Runtime Difficulty Changes =====
    public void IncreaseDifficulty()
    {
        if (currentDifficultyIndex < difficultyLevels.Length - 1)
        {
            SetDifficulty(currentDifficultyIndex + 1);
        }
    }

    public void DecreaseDifficulty()
    {
        if (currentDifficultyIndex > 0)
        {
            SetDifficulty(currentDifficultyIndex - 1);
        }
    }

    // ===== Gradual Speed Increase (Progressive Difficulty) =====
    public void StartProgressiveDifficulty(float speedIncreaseRate = 0.1f, float maxSpeed = 2f)
    {
        StartCoroutine(ProgressiveSpeedIncrease(speedIncreaseRate, maxSpeed));
    }

    private System.Collections.IEnumerator ProgressiveSpeedIncrease(float rate, float maxSpeed)
    {
        float startSpeed = currentDifficulty.musicSpeed;
        float timeElapsed = 0f;

        while (music.isPlaying && currentDifficulty.musicSpeed < maxSpeed)
        {
            timeElapsed += Time.deltaTime;
            float newSpeed = startSpeed + (rate * timeElapsed / 60f); // Increase per minute

            // Update current difficulty dynamically
            currentDifficulty.musicSpeed = Mathf.Min(newSpeed, maxSpeed);
            currentDifficulty.spawnSpeedMultiplier = currentDifficulty.musicSpeed;

            music.pitch = currentDifficulty.musicSpeed;
            effectiveTileFallTime = baseTileFallTime / currentDifficulty.spawnSpeedMultiplier;

            yield return new WaitForSeconds(1f); // Update every second
        }
    }

    // ===== Pause / Resume =====
    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;
        pauseSongTime = music.time;
        music.Pause();
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;
        music.time = pauseSongTime;
        music.pitch = currentDifficulty.musicSpeed; // Restore speed
        music.Play();
        Time.timeScale = 1f;
    }

    public void StopAll()
    {
        prepared = false;
        tilePool.ReturnAllTiles();

        if (music != null && music.isPlaying)
            music.Stop();

        StopAllCoroutines(); // Stop progressive difficulty
    }

    // ===== Getters =====
    public DifficultyLevel GetCurrentDifficulty() => currentDifficulty;
    public string GetCurrentDifficultyName() => currentDifficulty.name;
    public float GetEffectiveTileFallTime() => effectiveTileFallTime;

    // ===== Debug =====
    [ContextMenu("Test Difficulty Up")]
    void TestDifficultyUp() => IncreaseDifficulty();

    [ContextMenu("Test Difficulty Down")]
    void TestDifficultyDown() => DecreaseDifficulty();
}