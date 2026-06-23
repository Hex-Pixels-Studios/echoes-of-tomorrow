using System;

public static class GameEvents
{
    public static event Action OnMatchStarted;
    public static event Action OnMatchEnded;
    public static event Action<int> OnPlayerKilled;

    public static event Action<int> OnEchoSpawned;
    public static event Action<int> OnEchoCollected;
    public static event Action<int> OnUpgradeChosen;

    public static event Action OnCountdownTick;
    public static event Action OnCountdownEnd;

    public static void MatchStarted() => OnMatchStarted?.Invoke();

    public static void MatchEnded() => OnMatchEnded?.Invoke();

    public static void PlayerKilled(int playerIndex) => OnPlayerKilled?.Invoke(playerIndex);

    public static void EchoSpawned(int playerIndex) => OnEchoSpawned?.Invoke(playerIndex);

    public static void EchoCollected(int playerIndex) => OnEchoCollected?.Invoke(playerIndex);

    public static void UpgradeChosen(int playerIndex) => OnUpgradeChosen?.Invoke(playerIndex);

    public static void CountdownTick() => OnCountdownTick?.Invoke();

    public static void CountdownEnd() => OnCountdownEnd?.Invoke();
}
