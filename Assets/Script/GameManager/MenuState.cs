using UnityEngine;

// MenuState - example placeholder
public class MenuState : IGameState
{
    private GameSaveManager gameSaveManager;
    public MenuState(GameSaveManager gameSaveManager)
    {
        this.gameSaveManager = gameSaveManager;
    }
    public void Enter()
    {
        Debug.Log("Enter MenuState");
        // show menu UI
        UIManager.Instance.ShowMenu();
        // Load all saved data when game starts
        SoundController.Instance.PlayMusic(SoundName.BackgroundMusic, true); // Play menu music
    }

    public void Update()
    {
        // menu logic
    }

    public void Exit()
    {
        Debug.Log("Exit MenuState");
        // hide menu UI
        //UIManager.Instance.HideMenu();
        //SoundController.Instance.StopMusic(SoundName.BackgroundMusic); // Stop menu music
    }
}