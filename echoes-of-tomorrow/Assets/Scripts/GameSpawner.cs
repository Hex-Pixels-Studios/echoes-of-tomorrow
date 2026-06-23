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
            Debug.LogError(" no player prefab in game spawne");
            return;
        }

        var gamepads = Gamepad.all;
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        GameObject p1Obj = Instantiate(playerPrefab, spawnPointP1.position, spawnPointP1.rotation);
        p1Obj.name = "Player1";
        var p1Input = p1Obj.GetComponent<PlayerInput>();

        GameObject p2Obj = Instantiate(playerPrefab, spawnPointP2.position, spawnPointP2.rotation);
        p2Obj.name = "Player2";
        var p2Input = p2Obj.GetComponent<PlayerInput>();

        if (p1UsesKeyboardMouse)
        {
            if (keyboard != null && mouse != null)
                p1Input.SwitchCurrentControlScheme("KeyboardMouse", keyboard, mouse);
            if (gamepads.Count > 0)
                p2Input.SwitchCurrentControlScheme("Gamepad", gamepads[0]);
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
                p1Input.SwitchCurrentControlScheme("Gamepad", gamepads[0]);
                if (keyboard != null && mouse != null)
                    p2Input.SwitchCurrentControlScheme("KeyboardMouse", keyboard, mouse);
            }
        }

        var p1Controller = p1Obj.GetComponent<PlayerController>();
        var p2Controller = p2Obj.GetComponent<PlayerController>();

        p1Obj.GetComponent<UpgradeExecutor>()?.SetOpponent(p2Controller);
        p2Obj.GetComponent<UpgradeExecutor>()?.SetOpponent(p1Controller);

        Debug.Log($"p1: {p1Input.currentControlScheme} | p2: {p2Input.currentControlScheme}");
    }
}
