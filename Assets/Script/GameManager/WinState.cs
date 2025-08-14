using UnityEngine;
using UnityEngine.SceneManagement;

public class WinState : IGameState
{
    private GameSaveManager gameSaveManager;
    private ScoreManager scoreManager;
    public WinState(GameSaveManager gameSaveManager, ScoreManager scoreManager)
    {
        this.gameSaveManager = gameSaveManager;
        this.scoreManager = scoreManager;
    }
    public void Enter()
    {
        Debug.Log("You Win!");
        UIManager.Instance.ShowWin();
        scoreManager.EndGame(); // Finalize score calculations
        SoundController.Instance.PlaySound(SoundName.Win, false); // Play win sound effect
    }

    public void Update() { }

    public void Exit() { }
}
