#nullable enable
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // --- Core Game Variables ---
    [Header("Core Stats")]
    public int confidence = 0;
    public int dateScore = 0;
    public bool betaBehaviorDetected = false;

    // --- UI References ---
    [Header("UI Elements")]
    public GameObject betaBehaviorPanel;

    [Header("Panel CanvasGroups")]
    public CanvasGroup scorePanelCanvasGroup;
    public CanvasGroup confidencePanelCanvasGroup;

    [Header("Flash Settings")]
    public float flashDuration = 0.2f;
    public int flashCount = 3;
    public float holdTime = 1f;
    public float fadeTime = 1f;

    [Header("Alphavision Slide-In")]
    public RectTransform alphavisionRoot;
    public Vector2 visorStartPos = new Vector2(0, 800);
    public Vector2 visorEndPos = new Vector2(0, 0);
    public float visorSlideDuration = 1.2f;
    public float visorSlideDelay = 0.2f;

    [Header("Date Portrait Animation")]
    [SerializeField] private Image datePortrait;
    [SerializeField] private float dateFadeDuration = 1.5f; // seconds

    [Header("Fade to Black")]
    [SerializeField] private Image fadeOverlay;
    [SerializeField] private float fadeDuration = 1.5f;

    // --- Internals ---
    private CanvasGroup? betaCanvasGroup;
    private Coroutine? betaFlashRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (betaBehaviorPanel != null)
            betaCanvasGroup = betaBehaviorPanel.GetComponent<CanvasGroup>();

        if (scorePanelCanvasGroup != null)
            scorePanelCanvasGroup.alpha = 0;
        if (confidencePanelCanvasGroup != null)
            confidencePanelCanvasGroup.alpha = 0;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            ReloadScene();

        if (Input.GetKeyDown(KeyCode.Escape))
            QuitGame();
    }

    private void ReloadScene()
    {
        StartCoroutine(ReloadAfterDelay(0.5f));
    }

    private IEnumerator ReloadAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        var runner = FindObjectOfType<DialogueRunner>();
        if (runner != null)
            runner.Stop();

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    private void QuitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    // --- Yarn Commands ---
    [YarnCommand("RunStartupFlicker")]
    public void RunStartupFlicker()
    {
        StartCoroutine(StartupFlicker());
    }

    [YarnCommand("dateEnters")]
    public void DateEnters()
    {
        StartCoroutine(FadeInDatePortrait());
    }

    [YarnCommand("fadeToBlack")]
    public void FadeToBlack()
    {
        StartCoroutine(FadeScreenToBlack());
    }

    [YarnCommand("trigger_beta_warning")]
    public void TriggerBetaWarning()
    {
        if (betaFlashRoutine != null)
            StopCoroutine(betaFlashRoutine);

        betaFlashRoutine = StartCoroutine(BetaFlashRoutine());
    }

    // --- Coroutines ---

    private IEnumerator StartupFlicker()
    {
        // Slide in visor
        if (alphavisionRoot != null)
        {
            alphavisionRoot.anchoredPosition = visorStartPos;
            yield return new WaitForSeconds(visorSlideDelay);

            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / visorSlideDuration;
                float eased = Mathf.SmoothStep(0, 1, t);
                alphavisionRoot.anchoredPosition = Vector2.Lerp(visorStartPos, visorEndPos, eased);
                yield return null;
            }
        }

        // Flicker in panels
        yield return StartCoroutine(FlickerPanel(scorePanelCanvasGroup));
        yield return StartCoroutine(FlickerPanel(confidencePanelCanvasGroup));
    }

    private IEnumerator FadeInDatePortrait()
    {
        if (datePortrait == null)
        {
            Debug.LogWarning("Date portrait not assigned!");
            yield break;
        }

        Color c = datePortrait.color;
        c.a = 0f;
        datePortrait.color = c;
        datePortrait.gameObject.SetActive(true);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / dateFadeDuration;
            c.a = Mathf.SmoothStep(0f, 1f, t);
            datePortrait.color = c;
            yield return null;
        }

        c.a = 1f;
        datePortrait.color = c;
    }

    private IEnumerator FlickerPanel(CanvasGroup? panel)
    {
        if (panel == null) yield break;

        for (int i = 0; i < flashCount; i++)
        {
            panel.alpha = 1;
            yield return new WaitForSeconds(flashDuration);
            panel.alpha = 0;
            yield return new WaitForSeconds(flashDuration);
        }

        panel.alpha = 1;
    }

    private IEnumerator FadeScreenToBlack()
    {
        if (fadeOverlay == null)
        {
            Debug.LogWarning("Fade overlay not assigned!");
            yield break;
        }

        fadeOverlay.gameObject.SetActive(true);
        Color c = fadeOverlay.color;
        c.a = 0f;
        fadeOverlay.color = c;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            c.a = Mathf.SmoothStep(0f, 1f, t);
            fadeOverlay.color = c;
            yield return null;
        }

        c.a = 1f;
        fadeOverlay.color = c;
    }

    private IEnumerator BetaFlashRoutine()
    {
        if (betaCanvasGroup == null)
            yield break;

        betaBehaviorPanel.SetActive(true);
        betaCanvasGroup.alpha = 0;

        // 1. Flicker effect
        for (int i = 0; i < flashCount; i++)
        {
            betaCanvasGroup.alpha = 1f;
            yield return new WaitForSeconds(flashDuration);
            betaCanvasGroup.alpha = 0f;
            yield return new WaitForSeconds(flashDuration);
        }

        // 2. Hold fully visible
        betaCanvasGroup.alpha = 1f;
        yield return new WaitForSeconds(holdTime);

        // 3. Fade out smoothly
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            betaCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
            yield return null;
        }

        betaCanvasGroup.alpha = 0f;
        betaBehaviorPanel.SetActive(false);
    }
}
