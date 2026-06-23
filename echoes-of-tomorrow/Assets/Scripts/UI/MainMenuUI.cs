using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    CanvasGroup mainPanel;

    [SerializeField]
    CanvasGroup howToPlayPanel;

    [SerializeField]
    CanvasGroup controlsPanel;

    [SerializeField]
    CanvasGroup creditsPanel;

    [Header("Main Menu")]
    [SerializeField]
    Button startButton;

    [SerializeField]
    Button creditsButton;

    [SerializeField]
    Button quitButton;

    [Header("How To Play")]
    [SerializeField]
    Button howToPlayNextButton;

    [SerializeField]
    Button howToPlayBackButton;

    [Header("Controls")]
    [SerializeField]
    Button controlsNextButton;

    [SerializeField]
    Button controlsBackButton;

    [Header("Credits")]
    [SerializeField]
    Button creditsBackButton;

    [SerializeField]
    CreditsScroller creditsScroller;

    [Header("Scene")]
    [SerializeField]
    string gameSceneName = "Game";

    void Awake()
    {
        SetGroup(mainPanel, true);

        SetGroup(howToPlayPanel, false);
        SetGroup(controlsPanel, false);
        SetGroup(creditsPanel, false);
    }

    void Start()
    {
        startButton?.onClick.AddListener(OnStartGame);
        creditsButton?.onClick.AddListener(OnShowCredits);
        quitButton?.onClick.AddListener(OnQuit);

        howToPlayNextButton?.onClick.AddListener(OnHowToPlayNext);
        howToPlayBackButton?.onClick.AddListener(OnHowToPlayBack);

        controlsNextButton?.onClick.AddListener(OnControlsNext);
        controlsBackButton?.onClick.AddListener(OnControlsBack);

        creditsBackButton?.onClick.AddListener(OnBackFromCredits);

        AddHoverSounds(
            new List<Button>
            {
                startButton,
                creditsButton,
                quitButton,
                howToPlayNextButton,
                howToPlayBackButton,
                controlsNextButton,
                controlsBackButton,
                creditsBackButton,
            }
        );

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusic);
        }
    }

    void OnStartGame()
    {
        PlayClick();
        ShowPanel(mainPanel, howToPlayPanel);
    }

    void OnHowToPlayNext()
    {
        PlayClick();
        ShowPanel(howToPlayPanel, controlsPanel);
    }

    void OnHowToPlayBack()
    {
        PlayClick();
        ShowPanel(howToPlayPanel, mainPanel);
    }

    void OnControlsNext()
    {
        PlayClick();
        SceneManager.LoadScene(gameSceneName);
    }

    void OnControlsBack()
    {
        PlayClick();
        ShowPanel(controlsPanel, howToPlayPanel);
    }

    void OnShowCredits()
    {
        PlayClick();
        ShowPanel(mainPanel, creditsPanel);

        if (creditsScroller != null)
            creditsScroller.ResetScroll();
    }

    void OnBackFromCredits()
    {
        PlayClick();
        ShowPanel(creditsPanel, mainPanel);
    }

    void OnQuit()
    {
        PlayClick();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void ShowPanel(CanvasGroup hide, CanvasGroup show)
    {
        SetGroup(hide, false);
        SetGroup(show, true);
    }

    void SetGroup(CanvasGroup cg, bool active)
    {
        if (cg == null)
            return;

        cg.gameObject.SetActive(active);
        cg.alpha = active ? 1f : 0f;
        cg.interactable = active;
        cg.blocksRaycasts = active;
    }

    void PlayClick()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySfx(AudioManager.Instance.buttonClickClip);
    }

    void AddHoverSounds(List<Button> buttons)
    {
        foreach (var btn in buttons)
        {
            if (btn == null)
                continue;

            var trigger = btn.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            var entry = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter,
            };

            entry.callback.AddListener(_ =>
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySfx(AudioManager.Instance.buttonHoverClip, 0.5f);
            });

            trigger.triggers.Add(entry);
        }
    }
}
