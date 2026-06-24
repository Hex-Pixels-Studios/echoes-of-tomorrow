using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinScreenUI : MonoBehaviour
{
    [Header("panels")]
    [SerializeField]
    CanvasGroup rootGroup;

    [SerializeField]
    CanvasGroup glitchOverlay;

    [SerializeField]
    CanvasGroup mainPanel;

    [SerializeField]
    CanvasGroup creditsPanel;

    [Header("winner text")]
    [SerializeField]
    TMP_Text winnerLabel;

    [SerializeField]
    TMP_Text winTagline;

    [SerializeField]
    TMP_Text rematchCountdown;

    [Header("decorative")]
    [SerializeField]
    Image accentBar;

    [SerializeField]
    RectTransform glitchLine;

    [Header("buttons")]
    [SerializeField]
    Button mainMenuButton;

    [SerializeField]
    Button creditsButton;

    [SerializeField]
    Button quitButton;

    [SerializeField]
    Button creditsBackButton;

    [Header("credits")]
    [SerializeField]
    CreditsScroller creditsScroller;

    [Header("colors")]
    [SerializeField]
    Color p1Color = new Color(0.2f, 0.8f, 1f);

    [SerializeField]
    Color p2Color = new Color(1f, 0.4f, 0.1f);

    [Header("scene names")]
    [SerializeField]
    string mainMenuScene = "MainMenu";

    [Header("timing")]
    [SerializeField]
    float flashDuration = 0.08f;

    [SerializeField]
    float fadeInDuration = 0.3f;

    [SerializeField]
    float countdownDuration = 3f;

    Coroutine activeRoutine;
    Coroutine countdownRoutine;

    void Awake()
    {
        Hide();
        HideCredits();
    }

    void Start()
    {
        mainMenuButton?.onClick.AddListener(OnMainMenu);
        creditsButton?.onClick.AddListener(OnShowCredits);
        quitButton?.onClick.AddListener(OnQuit);
        creditsBackButton?.onClick.AddListener(OnHideCredits);
    }

    public void Show(PlayerController.PlayerID winner)
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(ShowRoutine(winner));
    }

    public void Hide()
    {
        if (rootGroup == null)
            return;
        rootGroup.alpha = 0f;
        rootGroup.interactable = false;
        rootGroup.blocksRaycasts = false;
        rootGroup.gameObject.SetActive(false);
    }

    IEnumerator ShowRoutine(PlayerController.PlayerID winner)
    {
        Color playerColor = winner == PlayerController.PlayerID.P1 ? p1Color : p2Color;
        string playerName = winner == PlayerController.PlayerID.P1 ? "PLAYER 1" : "PLAYER 2";

        if (winnerLabel != null)
        {
            winnerLabel.text = playerName;
            winnerLabel.color = playerColor;
        }
        if (accentBar != null)
            accentBar.color = playerColor;

        rootGroup.gameObject.SetActive(true);
        HideCredits();

        if (glitchOverlay != null)
        {
            glitchOverlay.gameObject.SetActive(true);
            glitchOverlay.alpha = 1f;
            yield return new WaitForSecondsRealtime(flashDuration);
            yield return StartCoroutine(FadeGroup(glitchOverlay, 0f, 0.15f));
            glitchOverlay.gameObject.SetActive(false);
        }

        rootGroup.alpha = 0f;
        yield return StartCoroutine(FadeGroup(rootGroup, 1f, fadeInDuration));
        rootGroup.interactable = true;
        rootGroup.blocksRaycasts = true;

        if (winnerLabel != null)
            yield return StartCoroutine(PunchScale(winnerLabel.transform, 1.4f, 0.22f));
        if (winTagline != null)
            yield return StartCoroutine(FadeInTMP(winTagline, 0.25f));

        StartCoroutine(GlitchLineLoop());

        if (countdownRoutine != null)
            StopCoroutine(countdownRoutine);
        countdownRoutine = StartCoroutine(RunCountdown());
    }

    IEnumerator RunCountdown()
    {
        if (rematchCountdown == null)
            yield break;

        rematchCountdown.gameObject.SetActive(true);
        float elapsed = 0f;

        while (elapsed < countdownDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            int left = Mathf.CeilToInt(countdownDuration - elapsed);
            rematchCountdown.text = $"REMATCH IN {left}...";
            yield return null;
        }

        rematchCountdown.text = "FIGHT!";
        yield return new WaitForSecondsRealtime(0.4f);

        yield return StartCoroutine(FadeGroup(rootGroup, 0f, 0.25f));
        Hide();
    }

    void CancelCountdown()
    {
        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
        }
        if (rematchCountdown != null)
            rematchCountdown.gameObject.SetActive(false);
    }

    void OnShowCredits()
    {
        CancelCountdown();
        StartCoroutine(TransitionToCredits());
    }

    void OnHideCredits()
    {
        StartCoroutine(TransitionFromCredits());
    }

    void OnMainMenu()
    {
        CancelCountdown();
        StartCoroutine(FadeAndLoad(mainMenuScene));
    }

    void OnQuit()
    {
        CancelCountdown();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator TransitionToCredits()
    {
        yield return StartCoroutine(FadeGroup(mainPanel, 0f, 0.2f));
        mainPanel.gameObject.SetActive(false);

        creditsPanel.gameObject.SetActive(true);
        creditsPanel.alpha = 0f;
        creditsPanel.interactable = false;
        creditsPanel.blocksRaycasts = false;

        creditsScroller?.ResetScroll();

        yield return StartCoroutine(FadeGroup(creditsPanel, 1f, 0.25f));
        creditsPanel.interactable = true;
        creditsPanel.blocksRaycasts = true;
    }

    IEnumerator TransitionFromCredits()
    {
        yield return StartCoroutine(FadeGroup(creditsPanel, 0f, 0.2f));
        HideCredits();

        mainPanel.gameObject.SetActive(true);
        mainPanel.alpha = 0f;
        mainPanel.interactable = false;
        mainPanel.blocksRaycasts = false;

        yield return StartCoroutine(FadeGroup(mainPanel, 1f, 0.25f));
        mainPanel.interactable = true;
        mainPanel.blocksRaycasts = true;
    }

    void HideCredits()
    {
        if (creditsPanel == null)
            return;
        creditsPanel.alpha = 0f;
        creditsPanel.interactable = false;
        creditsPanel.blocksRaycasts = false;
        creditsPanel.gameObject.SetActive(false);
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        yield return StartCoroutine(FadeGroup(rootGroup, 0f, 0.3f));
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator GlitchLineLoop()
    {
        if (glitchLine == null)
            yield break;
        var parent = (RectTransform)glitchLine.parent;

        while (rootGroup != null && rootGroup.alpha > 0.1f)
        {
            float h = parent.rect.height;
            glitchLine.anchoredPosition = new Vector2(0f, Random.Range(-h * 0.4f, h * 0.4f));
            glitchLine.sizeDelta = new Vector2(glitchLine.sizeDelta.x, Random.Range(1f, 4f));
            glitchLine.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(Random.Range(0.05f, 0.18f));
            glitchLine.gameObject.SetActive(false);
            yield return new WaitForSecondsRealtime(Random.Range(0.05f, 0.15f));
        }
    }

    IEnumerator PunchScale(Transform t, float peak, float duration)
    {
        t.localScale = Vector3.zero;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t01 = elapsed / duration;
            t.localScale = Vector3.one * Mathf.LerpUnclamped(0f, 1f, EaseOutBack(t01));
            yield return null;
        }
        t.localScale = Vector3.one;
    }

    IEnumerator FadeInTMP(TMP_Text label, float duration)
    {
        label.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            label.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        label.alpha = 1f;
    }

    IEnumerator FadeGroup(CanvasGroup cg, float target, float duration)
    {
        if (cg == null)
            yield break;
        float start = cg.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        cg.alpha = target;
    }

    float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
