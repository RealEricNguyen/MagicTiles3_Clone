using UnityEngine;

public class PausedState : IGameState
{
    //private AudioSource music;
    private BeatMapSpawner spawner;

    public PausedState(BeatMapSpawner spawner)
    {
        //this.music = music;
        this.spawner = spawner;
    }

    public void Enter()
    {
        Debug.Log("Enter PausedState");
        Time.timeScale = 0f;
        spawner.PauseGame();
        //f (music.isPlaying) music.Pause();
    }

    public void Update()
    {
        // wait for unpause
    }

    public void Exit()
    {
        Debug.Log("Exit PausedState");
        Time.timeScale = 1f;
        // Resume is now handled by PlayingState when we change state
    }

}