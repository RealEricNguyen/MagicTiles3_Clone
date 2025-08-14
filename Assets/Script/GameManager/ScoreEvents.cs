using System;

public static class ScoreEvents
{
    public static event Action OnPerfect;
    public static event Action OnGood;
    public static event Action OnMiss;

    public static void Perfect() => OnPerfect?.Invoke();
    public static void Good() => OnGood?.Invoke();
    public static void Miss() => OnMiss?.Invoke();
}