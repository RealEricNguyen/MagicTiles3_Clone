using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScoreManager : MonoBehaviour, ISaveableBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI References")]
    public Text scoreText;
    public Text feedbackText;
    public Text highScoreText; // Display high score

    [Header("Score Settings")]
    public int basePerfectScore = 300;
    public int baseGoodScore = 150;
    public float streakBonusMultiplier = 0.1f; // +10% per streak

    [Header("Save Data")]
    private GameData gameData = new GameData();

    // Current game state
    private int currentScore;
    private int perfectStreak = 0; // consecutive perfect counter

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Register for save system
            SaveableRegistry.Register(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Load(); // Load game data on start
        UpdateHighScoreDisplay();
    }

    void OnEnable()
    {
        ScoreEvents.OnPerfect += RegisterPerfect;
        ScoreEvents.OnGood += RegisterGood;
        ScoreEvents.OnMiss += RegisterMiss;
    }

    void OnDisable()
    {
        ScoreEvents.OnPerfect -= RegisterPerfect;
        ScoreEvents.OnGood -= RegisterGood;
        ScoreEvents.OnMiss -= RegisterMiss;
    }

    // ===== Score Event Handlers =====
    public void RegisterPerfect()
    {
        perfectStreak++;
        // Calculate score with streak bonus
        int bonusScore = Mathf.RoundToInt(basePerfectScore * (1 + (perfectStreak - 1) * streakBonusMultiplier));
        AddScore(bonusScore);

        if (perfectStreak > 1)
            ShowFeedback($"PERFECT x{perfectStreak}", Color.yellow);
        else
            ShowFeedback("PERFECT", Color.yellow);
    }

    public void RegisterGood()
    {
        perfectStreak = 0;
        AddScore(baseGoodScore);
        ShowFeedback("GOOD", Color.green);
    }

    public void RegisterMiss()
    {
        perfectStreak = 0;
        ShowFeedback("MISS", Color.red);
    }

    void AddScore(int amount)
    {
        currentScore += amount;
        if (scoreText != null)
            scoreText.text = "Score: " + currentScore.ToString();
    }

    void ShowFeedback(string message, Color color)
    {
        if (feedbackText == null) return;

        feedbackText.text = message;
        feedbackText.color = color;

        // Reset scale and alpha first
        feedbackText.rectTransform.localScale = Vector3.one;
        Color c = feedbackText.color;
        c.a = 1f;
        feedbackText.color = c;

        // Animate scale up + fade out
        Sequence seq = DOTween.Sequence();
        seq.Append(feedbackText.rectTransform.DOScale(1.5f, 0.1f).SetEase(Ease.OutQuad));
        seq.Join(feedbackText.DOFade(0f, 0.1f).SetEase(Ease.InQuad));
        seq.OnComplete(() =>
        {
            c.a = 1f;
            feedbackText.color = c;
        });
    }

    // ===== Game Management =====
    public void StartNewGame()
    {
        currentScore = 0;
        perfectStreak = 0;

        if (scoreText != null)
            scoreText.text = "Score: 0";

        Debug.Log("New game started, score reset");
    }

    public void EndGame()
    {
        // Check for new high score
        bool isNewHighScore = CheckAndUpdateHighScore();

        if (isNewHighScore)
        {
            Debug.Log($"🎉 NEW HIGH SCORE: {currentScore}!");
            ShowNewHighScoreEffect();
        }
        else
        {
            Debug.Log($"Game ended. Score: {currentScore}, High Score: {gameData.highScore}");
        }
    }

    bool CheckAndUpdateHighScore()
    {
        if (currentScore > gameData.highScore)
        {
            gameData.highScore = currentScore;
            UpdateHighScoreDisplay();
            Save(); // Save new high score immediately
            return true;
        }
        return false;
    }

    void UpdateHighScoreDisplay()
    {
        if (highScoreText != null)
        {
            highScoreText.text = "" + gameData.highScore.ToString();
        }
    }

    void ShowNewHighScoreEffect()
    {
        if (highScoreText != null)
        {
            // Animate the high score text
            Sequence seq = DOTween.Sequence();
            seq.Append(highScoreText.rectTransform.DOScale(1.3f, 0.3f).SetEase(Ease.OutBounce));
            seq.Append(highScoreText.rectTransform.DOScale(1f, 0.2f).SetEase(Ease.InQuad));

            // Flash color
            Color originalColor = highScoreText.color;
            seq.Join(highScoreText.DOColor(Color.yellow, 0.15f).SetLoops(4, LoopType.Yoyo));
        }

        // Show feedback
        ShowFeedback("NEW HIGH SCORE!", Color.cyan);
    }

    // ===== ISaveableBehaviour Implementation =====
    public void Save()
    {
        FileSaveSystem.SaveToFile("game_data", gameData);
        Debug.Log($"Saved game data: High Score = {gameData.highScore}, Difficulty = {gameData.selectedDifficultyIndex}");
    }

    public void Load()
    {
        var loadedData = FileSaveSystem.LoadFromFile<GameData>("game_data");

        if (loadedData != null)
        {
            gameData = loadedData;
            Debug.Log($"Loaded game data: High Score = {gameData.highScore}, Difficulty = {gameData.selectedDifficultyIndex}");
        }
        else
        {
            Debug.Log("No save file found, using default values");
            gameData = new GameData(); // Use defaults
        }
    }

    // ===== Public Getters =====
    public int GetCurrentScore() => currentScore;
    public int GetHighScore() => gameData.highScore;
    public int GetPerfectStreak() => perfectStreak;
    public int GetSelectedDifficultyIndex() => gameData.selectedDifficultyIndex;

    // ===== Difficulty Management (integrated) =====
    public void SetSelectedDifficultyIndex(int difficultyIndex)
    {
        gameData.selectedDifficultyIndex = difficultyIndex;
        Save(); // Auto-save difficulty change
    }

    // ===== Debug Methods =====
    [ContextMenu("Add 1000 Points")]
    void TestAddScore()
    {
        AddScore(1000);
    }

    [ContextMenu("Reset High Score")]
    void ResetHighScore()
    {
        gameData.highScore = 0;
        UpdateHighScoreDisplay();
        Save();
        Debug.Log("High score reset to 0");
    }

    [ContextMenu("End Game Test")]
    void TestEndGame()
    {
        AddScore(1000); // Add some test score
        EndGame();
    }

    private void OnDestroy()
    {
        SaveableRegistry.Unregister(this);
    }
}