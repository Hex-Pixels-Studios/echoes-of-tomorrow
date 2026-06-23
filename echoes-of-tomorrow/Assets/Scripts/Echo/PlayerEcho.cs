using System;
using UnityEngine;

public class PlayerEcho : MonoBehaviour
{
    PlayerController.PlayerID ownerID;
    Action<PlayerController.PlayerID> onCollected;

    public void Init(
        PlayerController.PlayerID id,
        Action<PlayerController.PlayerID> collectedCallback
    )
    {
        ownerID = id;
        onCollected = collectedCallback;
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
            return;

        if (player.ID == ownerID)
        {
            // correct player reached their echo
            Debug.Log($"{ownerID} collected their echo - trigger upgrade here later");
            onCollected?.Invoke(ownerID);
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"{player.ID} tried to collect {ownerID}'s echo - rejected");
        }
    }
}
