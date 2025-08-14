using UnityEngine;

public class PlayingState : IGameState
{
    private AudioSource music;
    private BeatMapSpawner spawner;
    private bool isResume;

    public PlayingState(AudioSource music, BeatMapSpawner spawner, bool isResume = false)
    {
        this.music = music;
        this.spawner = spawner;
        this.isResume = isResume;
    }

    public void Enter()
    {
        Debug.Log("Enter PlayingState");

        if (!isResume)
        {
            spawner.ResetAndPrepare();
            music.Play();
        }
        else
        {
            spawner.ResumeGame(); // resume tiles
            music.UnPause();      // resume audio
        }
    }

    public void Update()
    {
        if (!music.isPlaying && music.time > 0f)
        {
            GameManager.Instance.ShowWinGame();
        }
    }

    public void Exit()
    {
        Debug.Log("Exit PlayingState");
    }
}
