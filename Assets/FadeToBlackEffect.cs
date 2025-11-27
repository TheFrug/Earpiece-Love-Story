using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeToBlackEffect : MonoBehaviour
{
    public static FadeToBlackEffect Instance;   // Global access
    [SerializeField] private Image fadeImage;   // Fullscreen black image
    [SerializeField] private float fadeDuration = 1f;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        fadeImage.gameObject.SetActive(true);

        // Start visible if needed (main menu wants visible off)
        Color c = fadeImage.color;
        fadeImage.color = new Color(c.r, c.g, c.b, 0f);
    }

    public void FadeOut() => StartCoroutine(Fade(0f, 1f));
    public void FadeIn() => StartCoroutine(Fade(1f, 0f));

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeSceneRoutine(sceneName));
    }

    private IEnumerator FadeSceneRoutine(string sceneName)
    {
        // Fade to black
        yield return Fade(0f, 1f);

        // Load scene
        yield return SceneManager.LoadSceneAsync(sceneName);

        // Fade from black
        yield return Fade(1f, 0f);
    }

    public static bool IsFading { get; private set; }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        IsFading = true;

        fadeImage.gameObject.SetActive(true);
        float t = 0f;
        Color c = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, endAlpha, t / fadeDuration);
            fadeImage.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        fadeImage.color = new Color(c.r, c.g, c.b, endAlpha);

        if (endAlpha == 0f)
            fadeImage.gameObject.SetActive(false);

        IsFading = false;
    }

}
