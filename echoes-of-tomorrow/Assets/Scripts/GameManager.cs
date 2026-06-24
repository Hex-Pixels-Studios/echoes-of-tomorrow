using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("player prefabs")]
    [SerializeField]
    GameObject player1Prefab;

    [SerializeField]
    GameObject player2Prefab;

    [Header("spawn points")]
    [SerializeField]
    Transform spawnPointP1;

    [SerializeField]
    Transform spawnPointP2;

    [Header("split screen")]
    [SerializeField]
    SplitMode splitMode = SplitMode.SideBySide;

    [Header("win screen")]
    [SerializeField]
    GameObject winScreen;

    [SerializeField]
    TMP_Text winLabel;

    [SerializeField]
    float rematchDelay = 4f;

    public enum SplitMode
    {
        SideBySide,
        TopAndBottom,
    }

    [Header("win screen")]
    [SerializeField]
    WinScreenUI winScreenUI;

    PlayerController player1;
    PlayerController player2;
    bool roundActive;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (winScreen != null)
            winScreen.SetActive(false);
        StartCoroutine(SpawnNextFrame());
    }

    IEnumerator SpawnNextFrame()
    {
        yield return null;
        SpawnPlayers();
    }

    void SpawnPlayers()
    {
        if (player1Prefab == null || player2Prefab == null)
        {
            Debug.LogError("GameManager: assign both player prefabs");
            return;
        }

        GameObject p1Obj = Instantiate(player1Prefab, spawnPointP1.position, spawnPointP1.rotation);
        GameObject p2Obj = Instantiate(player2Prefab, spawnPointP2.position, spawnPointP2.rotation);

        p1Obj.name = "Player1";
        p2Obj.name = "Player2";

        player1 = p1Obj.GetComponent<PlayerController>();
        player2 = p2Obj.GetComponent<PlayerController>();

        if (player1 == null || player2 == null)
        {
            Debug.LogError("GameManager: player prefabs need a PlayerController");
            return;
        }

        // whoever dies  other wins
        player1.GetComponent<PlayerHealth>().OnDeath += () => DeclareWinner(player2);
        player2.GetComponent<PlayerHealth>().OnDeath += () => DeclareWinner(player1);

        AssignInputDevices(p1Obj.GetComponent<PlayerInput>(), p2Obj.GetComponent<PlayerInput>());
        SetupSplitScreen(player1, player2);

        roundActive = true;
        // Debug.Log("round started");
    }

    void DeclareWinner(PlayerController winner)
    {
        if (!roundActive)
            return;
        roundActive = false;

        Debug.Log($"{winner.name} wins");

        if (winScreen != null)
            winScreen.SetActive(true);
        if (winLabel != null)
            winLabel.text = $"{winner.name} wins!";

        StartCoroutine(RematchCountdown());
    }

    IEnumerator RematchCountdown()
    {
        yield return new WaitForSeconds(rematchDelay);
        if (winScreen != null)
            winScreen.SetActive(false);
        ResetRound();
    }

    void ResetRound()
    {
        ResetPlayer(player1, spawnPointP1);
        ResetPlayer(player2, spawnPointP2);
        roundActive = true;
        Debug.Log("round reset");
    }

    void ResetPlayer(PlayerController player, Transform spawnPoint)
    {
        if (player == null)
            return;
        player.gameObject.SetActive(true);

        var cc = player.GetComponent<CharacterController>();
        cc.enabled = false;
        player.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        cc.enabled = true;

        player.ResetPlayer();
    }

    void AssignInputDevices(PlayerInput p1Input, PlayerInput p2Input)
    {
        if (p1Input.actions == null)
        {
            Debug.LogError("GameManager: assign the InputActions asset on the player prefab");
            return;
        }

        Keyboard kb = null;
        List<Gamepad> gamepads = new();

        foreach (var device in InputSystem.devices)
        {
            if (device is Keyboard k)
                kb = k;
            else if (device is Gamepad g)
                gamepads.Add(g);
        }

        string kbScheme = FindSchemeName(p1Input, "keyboard");
        string padScheme = FindSchemeName(p1Input, "gamepad");

        StartCoroutine(SwitchSchemesNextFrame(p1Input, p2Input, kb, gamepads, kbScheme, padScheme));
    }

    IEnumerator SwitchSchemesNextFrame(
        PlayerInput p1Input,
        PlayerInput p2Input,
        Keyboard kb,
        List<Gamepad> gamepads,
        string kbScheme,
        string padScheme
    )
    {
        yield return null;
        yield return null;

        if (gamepads.Count >= 2 && padScheme != null)
        {
            p1Input.SwitchCurrentControlScheme(padScheme, gamepads[0]);
            p2Input.SwitchCurrentControlScheme(padScheme, gamepads[1]);
        }
        else if (gamepads.Count >= 1 && kb != null && kbScheme != null && padScheme != null)
        {
            p1Input.SwitchCurrentControlScheme(kbScheme, kb);
            p2Input.SwitchCurrentControlScheme(padScheme, gamepads[0]);
        }
        else if (kb != null && kbScheme != null)
        {
            Debug.LogWarning("GameManager: only keyboard - both players sharing it");
            p1Input.SwitchCurrentControlScheme(kbScheme, kb);
            p2Input.SwitchCurrentControlScheme(kbScheme, kb);
        }
        else
        {
            Debug.LogError("GameManager: no valid input devices found");
        }
    }

    string FindSchemeName(PlayerInput input, string keyword)
    {
        if (input.actions == null)
            return null;
        foreach (var scheme in input.actions.controlSchemes)
            if (scheme.name.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return scheme.name;
        return null;
    }

    void SetupSplitScreen(PlayerController p1, PlayerController p2)
    {
        Camera cam1 = p1.PlayerCamera;
        Camera cam2 = p2.PlayerCamera;

        if (cam1 == null || cam2 == null)
        {
            Debug.LogError("GameManager: player cameras null");
            return;
        }

        if (splitMode == SplitMode.SideBySide)
        {
            cam1.rect = new Rect(0f, 0f, 0.5f, 1f);
            cam2.rect = new Rect(0.5f, 0f, 0.5f, 1f);
        }
        else
        {
            cam1.rect = new Rect(0f, 0.5f, 1f, 0.5f);
            cam2.rect = new Rect(0f, 0f, 1f, 0.5f);
        }

        cam1.depth = 0;
        cam2.depth = 1;

        if (Camera.main != null && Camera.main != cam1 && Camera.main != cam2)
            Camera.main.gameObject.SetActive(false);
    }

    public PlayerController Player1 => player1;
    public PlayerController Player2 => player2;
    public bool RoundActive => roundActive;
}
