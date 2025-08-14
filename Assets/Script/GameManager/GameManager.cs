using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public bool hasLost = false;

    [Header("References")]
    public AudioSource music;
    public BeatMapSpawner spawner;
    public GameSaveManager gameSaveManager;
    public ScoreManager scoreManager; // Reference to ScoreManager

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        GameStateManager.Instance.ChangeState(new MenuState(gameSaveManager));
    }
    public void PlayGame()
    {
        GameStateManager.Instance.ChangeState(new PlayingState(music, spawner));
    }
    public void PauseGame()
    {
        GameStateManager.Instance.ChangeState(new PausedState(spawner));
    }
    public void ResumeGame()
    {
        // Return to PlayingState but without resetting
        GameStateManager.Instance.ChangeState(new PlayingState(music, spawner, true));
    }
    public void ShowWinGame()
    {
        GameStateManager.Instance.ChangeState(new WinState(gameSaveManager, scoreManager));
    }
    public void ShowLoseGame()
    {
        if (hasLost) return; // avoid multiple calls
        hasLost = true;
        GameStateManager.Instance.ChangeState(new LoseState(spawner,gameSaveManager, scoreManager));
    }
}