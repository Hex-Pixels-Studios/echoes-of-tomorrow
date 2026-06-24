using System;

public static class GameEvents
{
    // match flow
    public static event Action OnMatchStarted;
    public static event Action OnMatchEnded;

    // player
    public static event Action<int> OnPlayerKilled;
    public static event Action<int> OnPlayerHit;
    public static event Action<int> OnPlayerJump;
    public static event Action<int> OnPlayerLand;
    public static event Action<int> OnPlayerKnockback;

    // echoes
    public static event Action<int> OnEchoSpawned;
    public static event Action<int> OnEchoCollected;
    public static event Action<int> OnEchoRejected; // wrong player touched echo

    // upgrades / combat
    public static event Action<int> OnUpgradeChosen;
    public static event Action<int> OnProjectileFired;
    public static event Action<int> OnProjectileHit;
    public static event Action<int> OnGrenadeThrown;
    public static event Action<int> OnGrenadeExploded;
    public static event Action<int> OnIceFreeze;
    public static event Action<int> OnTeleport;

    // vitals
    public static event Action<int> OnPlayerHealed;
    public static event Action<int> OnStaminaRestored;

    // round / ui
    public static event Action OnCountdownTick;
    public static event Action OnCountdownEnd;
    public static event Action OnWinScreenOpened;

    // invokers
    public static void MatchStarted() => OnMatchStarted?.Invoke();

    public static void MatchEnded() => OnMatchEnded?.Invoke();

    public static void PlayerKilled(int i) => OnPlayerKilled?.Invoke(i);

    public static void PlayerHit(int i) => OnPlayerHit?.Invoke(i);

    public static void PlayerJump(int i) => OnPlayerJump?.Invoke(i);

    public static void PlayerLand(int i) => OnPlayerLand?.Invoke(i);

    public static void PlayerKnockback(int i) => OnPlayerKnockback?.Invoke(i);

    public static void EchoSpawned(int i) => OnEchoSpawned?.Invoke(i);

    public static void EchoCollected(int i) => OnEchoCollected?.Invoke(i);

    public static void EchoRejected(int i) => OnEchoRejected?.Invoke(i);

    public static void UpgradeChosen(int i) => OnUpgradeChosen?.Invoke(i);

    public static void ProjectileFired(int i) => OnProjectileFired?.Invoke(i);

    public static void ProjectileHit(int i) => OnProjectileHit?.Invoke(i);

    public static void GrenadeThrown(int i) => OnGrenadeThrown?.Invoke(i);

    public static void GrenadeExploded(int i) => OnGrenadeExploded?.Invoke(i);

    public static void IceFreeze(int i) => OnIceFreeze?.Invoke(i);

    public static void Teleport(int i) => OnTeleport?.Invoke(i);

    public static void PlayerHealed(int i) => OnPlayerHealed?.Invoke(i);

    public static void StaminaRestored(int i) => OnStaminaRestored?.Invoke(i);

    public static void CountdownTick() => OnCountdownTick?.Invoke();

    public static void CountdownEnd() => OnCountdownEnd?.Invoke();

    public static void WinScreenOpened() => OnWinScreenOpened?.Invoke();
}
