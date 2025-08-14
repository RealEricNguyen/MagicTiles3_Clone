using UnityEngine;

public class LoseState : IGameState
{
    private BeatMapSpawner spawner;
    private GameSaveManager gameSaveManager;
    private ScoreManager scoreManager;

    public LoseState(BeatMapSpawner spawner, GameSaveManager gameSaveManager, ScoreManager scoreManager)
    {
        this.spawner = spawner;
        this.gameSaveManager = gameSaveManager;
        this.scoreManager = scoreManager;
    }

    public void Enter()
    {
        Debug.Log("You Lose!");
        spawner.StopAll();
        UIManager.Instance.ShowLose();
        scoreManager.EndGame(); // Finalize score calculations
        SoundController.Instance.PlaySound(SoundName.Lose, false); // Play lose sound effect
    }

    public void Update() { }

    public void Exit() { }
}
