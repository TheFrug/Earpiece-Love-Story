using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class FadeToBlackEffect : MonoBehaviour
{
    public static FadeToBlackEffect Instance;   // Global access
    [SerializeField] private Image fadeImage;   // Fullscreen black image
    [SerializeField] private float fadeDuration = 1f;

    // --- NEW: optional callback when fade out finishes ---
    private Action? onFadeOutComplete;

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            // reset the fade image so it's invisible
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0f);
            fadeImage.gameObject.SetActive(false);
            IsFading = false;
        }
    }


    // --- EXISTING ---
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

    // --- NEW: helper method to register callback ---
    public void SetOnFadeOutComplete(Action callback)
    {
        onFadeOutComplete = callback;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        IsFading = true;

        fadeImage.gameObject.SetActive(true);
        float t = 0f;
        Color c = fadeImage.color;

        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            float a = Mathf.SmoothStep(startAlpha, endAlpha, Mathf.Clamp01(t));
            fadeImage.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        // Ensure final alpha is set
        fadeImage.color = new Color(c.r, c.g, c.b, endAlpha);

        if (endAlpha == 0f)
            fadeImage.gameObject.SetActive(false);

        IsFading = false;

        // Only call the callback after fade-out fully finished
        if (startAlpha == 0f && endAlpha == 1f)
        {
            // tiny yield to guarantee frame update? usually not necessary with SmoothStep
            yield return null;
            onFadeOutComplete?.Invoke();
            onFadeOutComplete = null;
        }
    }
}
