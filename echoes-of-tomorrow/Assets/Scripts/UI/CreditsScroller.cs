using System.Collections;
using UnityEngine;

public class CreditsScroller : MonoBehaviour
{
    [Header("scroll")]
    [SerializeField]
    RectTransform contentRect;

    [SerializeField]
    float scrollSpeed = 60f;

    [SerializeField]
    float startDelay = 1.2f;

    [SerializeField]
    bool loopCredits = false;

    [Header("speed control")]
    [SerializeField]
    float fastScrollSpeed = 200f;
    bool fastScrollActive = false;

    float startY;
    float endY;
    bool initialised = false;

    void OnEnable()
    {
        if (contentRect == null)
        {
            Debug.LogWarning("creditsscroller: no contentRect assigned");
            return;
        }

        if (!initialised)
        {
            startY = contentRect.anchoredPosition.y;
            initialised = true;
        }

        endY = startY + contentRect.rect.height + Screen.height;

        fastScrollActive = false;
        StopAllCoroutines();
        StartCoroutine(ScrollRoutine());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    void Update()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        bool held = kb != null && kb.anyKey.isPressed;

        if (held)
            fastScrollActive = true;
    }

    public void SetFastScroll(bool active)
    {
        fastScrollActive = active;
    }

    IEnumerator ScrollRoutine()
    {
        contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, startY);

        yield return new WaitForSecondsRealtime(startDelay);

        while (true)
        {
            float speed = fastScrollActive ? fastScrollSpeed : scrollSpeed;
            float newY = contentRect.anchoredPosition.y + speed * Time.unscaledDeltaTime;

            contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, newY);

            if (newY >= endY)
            {
                if (loopCredits)
                {
                    contentRect.anchoredPosition = new Vector2(
                        contentRect.anchoredPosition.x,
                        startY
                    );
                }
                else
                {
                    yield break;
                }
            }

            yield return null;
        }
    }

    public void ResetScroll()
    {
        if (contentRect == null)
            return;
        if (!gameObject.activeInHierarchy)
            return;
        StopAllCoroutines();
        fastScrollActive = false;
        StartCoroutine(ScrollRoutine());
    }
}
