using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    private CanvasGroup canvasGroup;
    private bool isFading;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        CreateOverlay();
    }

    public static SceneTransition EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        GameObject transitionObject = new GameObject("SceneTransition");
        return transitionObject.AddComponent<SceneTransition>();
    }

    private void CreateOverlay()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>();

        GameObject imageObject = new GameObject("FadeImage");
        imageObject.transform.SetParent(transform, false);

        RectTransform rectTransform = imageObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image image = imageObject.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0f);

        canvasGroup = imageObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    public IEnumerator FadeOut(float duration)
    {
        yield return Fade(1f, duration);
    }

    public IEnumerator FadeIn(float duration)
    {
        yield return Fade(0f, duration);
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        if (canvasGroup == null)
        {
            yield break;
        }

        if (isFading)
        {
            while (isFading)
            {
                yield return null;
            }
        }

        isFading = true;
        float startAlpha = canvasGroup.alpha;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        isFading = false;
    }
}
