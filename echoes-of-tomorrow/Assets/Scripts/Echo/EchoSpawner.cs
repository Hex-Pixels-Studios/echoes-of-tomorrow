using System.Collections;
using UnityEngine;

public class EchoSpawner : MonoBehaviour
{
    [Header("echo prefabs")]
    [SerializeField]
    GameObject p1EchoPrefab;

    [SerializeField]
    GameObject p2EchoPrefab;

    [Header("spawn points")]
    [SerializeField]
    Transform[] spawnPoints;

    [Header("timing")]
    [SerializeField]
    float spawnInterval = 20f;

    [SerializeField]
    float firstSpawnDelay = 5f; // short delay before first echo so round has time 2 start

    // track active echoes so  dont stack duplicates
    PlayerEcho activeP1Echo;
    PlayerEcho activeP2Echo;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(firstSpawnDelay);

        while (true)
        {
            SpawnEchoes();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEchoes()
    {
        SpawnEchoForPlayer(PlayerController.PlayerID.P1, ref activeP1Echo, p1EchoPrefab);
        SpawnEchoForPlayer(PlayerController.PlayerID.P2, ref activeP2Echo, p2EchoPrefab);
    }

    void SpawnEchoForPlayer(
        PlayerController.PlayerID id,
        ref PlayerEcho activeEcho,
        GameObject prefab
    )
    {
        if (prefab == null)
        {
            Debug.LogWarning($"EechoSpawner: no prefab assigned for {id}");
            return;
        }

        // if there's already an active echo for this player, destroy it first
        if (activeEcho != null)
        {
            Debug.Log($"{id} echo already active - replacing it");
            Destroy(activeEcho.gameObject);
        }

        Transform spawnPoint = GetRandomSpawnPoint();
        if (spawnPoint == null)
            return;

        GameObject echoObj = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        activeEcho = echoObj.GetComponent<PlayerEcho>();

        if (activeEcho == null)
        {
            // Debug.LogError($"EchoSpawner: {id} echo prefab is missing a PlayerEcho component");
            return;
        }

        activeEcho.Init(id, OnEchoCollected);
        // Debug.Log($"{id} echo spawned at {spawnPoint.name}");
    }

    Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            // Debug.LogError("EchoSpawner.. no spawn points assigned");
            return null;
        }
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    // donee by the echo when a player collects it
    void OnEchoCollected(PlayerController.PlayerID id)
    {
        if (id == PlayerController.PlayerID.P1)
            activeP1Echo = null;
        else
            activeP2Echo = null;
    }
}
