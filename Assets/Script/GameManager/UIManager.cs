using DG.Tweening;
using JetBrains.Rider.Unity.Editor;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("----Panel----")]
    public GameObject menuPanel;
    public GameObject pausePanel;
    public GameObject losePanel;
    public GameObject winPanel;
    public GameObject musicListPanel;

    [Header("----Button----")]
    public Button playButton;
    public Button pauseButton;
    public Button resumeButton;
    public Button restartWinButton;
    public Button restartLoseButton;
    public Button musicListButton; // Button to open music list panel

    [Header("----Difficulty----")]
    public Button leftButton;
    public Button rightButton;
    public Text levelDifficultText;

    [Header("----References----")]
    public GameManager gameManager;
    public BeatMapSpawner beatMapSpawner;
    public ScoreManager scoreManager; // Reference to ScoreManager

    [Header("----Text----")]
    public Text selectedMusicText; // Text to display selected music title
    public Text countdownText; // Assign in Inspector

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

    private void Start()
    {
        SetupButtonListeners();
        FindReferences();
        InitializeDifficulty();
    }

    void SetupButtonListeners()
    {
        playButton.onClick.AddListener(OnClickPlayGame);
        pauseButton.onClick.AddListener(OnClickPauseGame);
        resumeButton.onClick.AddListener(OnClickResumeGame);
        restartWinButton.onClick.AddListener(OnClickRestartGame);
        restartLoseButton.onClick.AddListener(OnClickRestartGame);
        musicListButton.onClick.AddListener(() => ShowMusicList());

        leftButton.onClick.AddListener(OnClickLeftDifficulty);
        rightButton.onClick.AddListener(OnClickRightDifficulty);
    }

    void FindReferences()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (beatMapSpawner == null)
            beatMapSpawner = FindObjectOfType<BeatMapSpawner>();

        if (scoreManager == null)
            scoreManager = FindObjectOfType<ScoreManager>();
    }

    void InitializeDifficulty()
    {
        if (scoreManager != null && beatMapSpawner != null)
        {
            // Load saved difficulty from ScoreManager
            int savedDifficulty = scoreManager.GetSelectedDifficultyIndex();
            beatMapSpawner.SetDifficulty(savedDifficulty);
            UpdateDifficultyUI();
        }
    }

    void OnClickLeftDifficulty()
    {
        if (beatMapSpawner == null || scoreManager == null) return;

        int currentIndex = scoreManager.GetSelectedDifficultyIndex();
        if (currentIndex > 0)
        {
            int newIndex = currentIndex - 1;
            beatMapSpawner.SetDifficulty(newIndex);
            scoreManager.SetSelectedDifficultyIndex(newIndex);
            UpdateDifficultyUI();
        }
        SoundController.Instance.PlaySound(SoundName.Click, false);
    }

    void OnClickRightDifficulty()
    {
        if (beatMapSpawner == null || scoreManager == null) return;

        int currentIndex = scoreManager.GetSelectedDifficultyIndex();
        if (currentIndex < beatMapSpawner.difficultyLevels.Length - 1)
        {
            int newIndex = currentIndex + 1;
            beatMapSpawner.SetDifficulty(newIndex);
            scoreManager.SetSelectedDifficultyIndex(newIndex);
            UpdateDifficultyUI();
        }
        SoundController.Instance.PlaySound(SoundName.Click, false);
    }

    void UpdateDifficultyUI()
    {
        if (beatMapSpawner == null || levelDifficultText == null || scoreManager == null) return;

        string difficultyName = beatMapSpawner.GetCurrentDifficultyName();
        levelDifficultText.text = difficultyName;

        int currentIndex = scoreManager.GetSelectedDifficultyIndex();

        // Update button states
        if (leftButton != null)
            leftButton.interactable = currentIndex > 0;

        if (rightButton != null)
            rightButton.interactable = currentIndex < beatMapSpawner.difficultyLevels.Length - 1;
    }

    // ===== UI Panel Methods =====
    public void ShowMenu() { menuPanel.SetActive(true); }
    public void HideMenu() { menuPanel.SetActive(false); }
    public void ShowPause() { pausePanel.SetActive(true); }
    public void HidePause() { pausePanel.SetActive(false); }
    public void ShowWin() { winPanel.SetActive(true); }
    public void ShowLose() { losePanel.SetActive(true); }
    public void ShowMusicList() { musicListPanel.SetActive(true); SoundController.Instance.PlaySound(SoundName.Click2, false); }
    public void HideMusicList(string musicName) 
    { 
        musicListPanel.SetActive(false);
        if (selectedMusicText != null)
        {
            // Clear selected music text when hiding the panel
            selectedMusicText.text = musicName;
        }
    }

    // ===== Game Control Methods =====
    void OnClickPlayGame()
    {
        if (scoreManager != null)
            scoreManager.StartNewGame();
        SoundController.Instance.PlaySound(SoundName.Playsound, false);
        HideMenu();
        SoundController.Instance.StopMusic(SoundName.BackgroundMusic); // Stop menu music
        // Run countdown first, then start the game
        StartCoroutine(ShowCountdown(() =>
        {
            gameManager.PlayGame();
        }));
    }

    public void OnClickPauseGame()
    {
        gameManager.PauseGame();
        ShowPause();
    }

    public void OnClickResumeGame()
    {
        gameManager.PlayGame();
        HidePause();
    }

    public void OnClickRestartGame()
    {
        SceneManager.LoadScene(0);
    }
    // ================= COUNTDOWN =================
    public IEnumerator ShowCountdown(System.Action onComplete)
    {
        if (countdownText == null)
        {
            Debug.LogWarning("CountdownText not assigned in UIManager!");
            onComplete?.Invoke();
            yield break;
        }

        countdownText.gameObject.SetActive(true);

        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();

            // Reset scale & alpha
            countdownText.rectTransform.localScale = Vector3.one;
            Color c = countdownText.color;
            c.a = 1f;
            countdownText.color = c;

            // Animate: scale up + fade out
            Sequence seq = DOTween.Sequence();
            seq.Append(countdownText.rectTransform.DOScale(1.5f, 0.5f).SetEase(Ease.OutQuad));
            seq.Join(countdownText.DOFade(0f, 0.5f).SetEase(Ease.InQuad));
            yield return seq.WaitForCompletion();
        }

        countdownText.gameObject.SetActive(false);

        // Call the callback (start game) after countdown
        onComplete?.Invoke();
    }
}