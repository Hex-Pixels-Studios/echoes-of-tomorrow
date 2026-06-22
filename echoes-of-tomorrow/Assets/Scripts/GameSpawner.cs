using UnityEngine;
using UnityEngine.InputSystem;

public class GameSpawner : MonoBehaviour
{
    [Header("prefab")]
    [SerializeField]
    GameObject playerPrefab;

    [Header("spawn points")]
    [SerializeField]
    Transform spawnPointP1;

    [SerializeField]
    Transform spawnPointP2;

    [Header("control schemes")]
    [SerializeField]
    bool p1UsesKeyboardMouse = true;

    void Start()
    {
        SpawnPlayers();
    }

    void SpawnPlayers()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("GameSpawner... no player prefab assigned");
            return;
        }

        var gamepads = Gamepad.all;
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        GameObject p1 = Instantiate(playerPrefab, spawnPointP1.position, spawnPointP1.rotation);
        p1.name = "Player1";
        var p1Input = p1.GetComponent<PlayerInput>();

        GameObject p2 = Instantiate(playerPrefab, spawnPointP2.position, spawnPointP2.rotation);
        p2.name = "Player2";
        var p2Input = p2.GetComponent<PlayerInput>();

        if (p1UsesKeyboardMouse)
        {
            if (keyboard != null && mouse != null)
                p1Input.SwitchCurrentControlScheme("KeyboardMouse", keyboard, mouse);

            if (gamepads.Count > 0)
                p2Input.SwitchCurrentControlScheme("Gamepad", gamepads[0]);
            else
                Debug.LogWarning("GameSpawner no gamepad found for p2");
        }
        else
        {
            if (gamepads.Count >= 2)
            {
                p1Input.SwitchCurrentControlScheme("Gamepad", gamepads[0]);
                p2Input.SwitchCurrentControlScheme("Gamepad", gamepads[1]);
            }
            else if (gamepads.Count == 1)
            {
                Debug.LogWarning(
                    "GameSpawner: only one gamepad, giving p2 keyboard+mouse as fallbackyy"
                );
                p1Input.SwitchCurrentControlScheme("Gamepad", gamepads[0]);
                if (keyboard != null && mouse != null)
                    p2Input.SwitchCurrentControlScheme("KeyboardMouse", keyboard, mouse);
            }
            else { }
        }
    }
}
