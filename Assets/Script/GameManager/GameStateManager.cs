using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    private IGameState currentState;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        currentState?.Update();
    }

    public void ChangeState(IGameState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }
}