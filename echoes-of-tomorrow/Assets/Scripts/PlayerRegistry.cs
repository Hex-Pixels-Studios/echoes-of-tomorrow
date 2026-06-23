using System.Collections.Generic;
using UnityEngine;


public static class PlayerRegistry
{
    static readonly Dictionary<PlayerController.PlayerID, PlayerController> players = new();

    public static void Register(PlayerController player)
    {
        players[player.ID] = player;
        Debug.Log($"PlayerRegistry: {player.ID} registered");
    }

    public static void Unregister(PlayerController player)
    {
        players.Remove(player.ID);
    }

    public static PlayerController Get(PlayerController.PlayerID id)
    {
        players.TryGetValue(id, out var player);
        return player;
    }

    // convenience - get the opponent of a given player
    public static PlayerController GetOpponent(PlayerController.PlayerID id)
    {
        var opponentID = id == PlayerController.PlayerID.P1
            ? PlayerController.PlayerID.P2
            : PlayerController.PlayerID.P1;
        return Get(opponentID);
    }
}
