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
    public TMP_Text confidenceText;
    public TMP_Text dateScoreText;
    public TMP_Text feedbackText;
    public TMP_Text scoreChangeText; // floating +10 “reason” text
    public GameObject feedbackPanel;
    public GameObject betaBehaviorPanel;

    [Header("Panel CanvasGroups")]
    public CanvasGroup scorePanelCanvasGroup;
    public CanvasGroup confidencePanelCanvasGroup;
    public CanvasGroup feedbackPanelCanvasGroup;

    [Header("Flash Settings")]
    public float flashDuration = 0.2f;
    public int flashCount = 3;
    public float holdTime = 1f;
    public float fadeTime = 1f;

    [Header("Feedback Animation")]
    public float feedbackSlideDuration = 0.6f;
    public Vector2 feedbackOnScreenPos = new Vector2(0, 0);
    public Vector2 feedbackOffScreenPos = new Vector2(0, -250);

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

    // --- Score Color Thresholds ---
    [Header("Score Colors (Low → High)")]
    public Color scoreColor0to59 = new Color(0.8f, 0.2f, 0.2f);   // red
    public Color scoreColor60to69 = new Color(1f, 0.5f, 0f);      // orange
    public Color scoreColor70to79 = new Color(1f, 0.8f, 0f);      // yellow
    public Color scoreColor80to89 = new Color(0.85f, 1f, 0.4f);   // yellow-green
    public Color scoreColor90to100 = new Color(0.02f, 0.87f, 0.45f); // green (#05DF72)

    private CanvasGroup betaCanvasGroup;
    private Coroutine betaFlashRoutine;
    private Coroutine feedbackRoutine;
    private RectTransform feedbackRect;

    private bool isFeedbackVisible = false;

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
        if (feedbackPanel != null)
            feedbackRect = feedbackPanel.GetComponent<RectTransform>();

        if (scorePanelCanvasGroup != null) scorePanelCanvasGroup.alpha = 0;
        if (confidencePanelCanvasGroup != null) confidencePanelCanvasGroup.alpha = 0;
        if (feedbackPanelCanvasGroup != null) feedbackPanelCanvasGroup.alpha = 0;
    }

    private void Update()
    {
        // ENTER → reload the scene
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadScene();
        }

        // ESC → quit the application
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }

    private void ReloadScene()
    {
        StartCoroutine(ReloadAfterDelay(0.5f));
    }

    private IEnumerator ReloadAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Stop all Yarn coroutines safely
        var runner = FindObjectOfType<Yarn.Unity.DialogueRunner>();
        if (runner != null)
            runner.Stop();

        // Reload
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    private void QuitGame()
    {
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // stop playmode if testing
    #else
            Application.Quit(); // close the built app
    #endif
    }

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

    // --- UI Update Methods ---

    public void SetConfidence(int newValue)
    {
        confidence = Mathf.Clamp(newValue, 0, 100);
        if (confidenceText != null)
            confidenceText.text = $"Confidence: {confidence}%";
    }

    public void SetDateScore(int newValue)
    {
        dateScore = Mathf.Clamp(newValue, 0, 100);

        if (dateScoreText != null)
        {
            dateScoreText.text = $"{dateScore}";
            dateScoreText.color = GetScoreColor(dateScore);
        }
    }

    // --- Score Commands for Yarn ---
    [YarnCommand("increase_score")]
    public void IncreaseScore(int amount, string reason)
    {
        StartCoroutine(ScoreChangeRoutine(amount, reason, true));
    }

    [YarnCommand("decrease_score")]
    public void DecreaseScore(int amount, string reason)
    {
        StartCoroutine(ScoreChangeRoutine(amount, reason, false));
    }

    private IEnumerator ScoreChangeRoutine(int amount, string reason, bool isIncrease)
    {
        int newScore = isIncrease ? dateScore + amount : dateScore - amount;
        SetDateScore(newScore);

        if (scoreChangeText != null)
        {
            scoreChangeText.text = (isIncrease ? "+" : "-") + amount + "  " + reason;
            scoreChangeText.color = isIncrease ? scoreColor90to100 : scoreColor0to59;
            scoreChangeText.alpha = 1f;

            Vector3 startPos = scoreChangeText.rectTransform.anchoredPosition;
            Vector3 endPos = startPos + new Vector3(0, 80f, 0); // move slower and higher
            float duration = 2.5f; // slower total movement

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;

                // Smooth motion and fade
                float easedT = Mathf.SmoothStep(0, 1, t);
                scoreChangeText.rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, easedT);
                scoreChangeText.alpha = Mathf.Lerp(1f, 0f, Mathf.Pow(t, 1.5f)); // fade slower at start, faster at end

                yield return null;
            }

            scoreChangeText.text = "";
            scoreChangeText.rectTransform.anchoredPosition = startPos;
        }
    }

    private Color GetScoreColor(int score)
    {
        if (score < 60) return scoreColor0to59;
        if (score < 70) return scoreColor60to69;
        if (score < 80) return scoreColor70to79;
        if (score < 90) return scoreColor80to89;
        return scoreColor90to100;
    }


    // --- Yarn command for feedback (Slide In) ---
    [YarnCommand("set_feedback")]
    public void SetFeedbackText(string newText)
    {
        if (feedbackRoutine != null)
            StopCoroutine(feedbackRoutine);

        // If feedback is already visible, just update text (no re-animation)
        if (isFeedbackVisible)
        {
            feedbackText.text = newText;
        }
        else
        {
            feedbackRoutine = StartCoroutine(AnimateFeedbackIn(newText));
        }
    }

    // --- Yarn command for feedback slide out ---
    [YarnCommand("feedback_slide_out")]
    public void FeedbackSlideOut()
    {
        if (feedbackRoutine != null)
            StopCoroutine(feedbackRoutine);
        feedbackRoutine = StartCoroutine(AnimateFeedbackOut());
    }

    // --- Feedback panel slide in ---
    private IEnumerator AnimateFeedbackIn(string newText)
    {
        if (feedbackRect == null || feedbackText == null)
            yield break;

        isFeedbackVisible = true;
        feedbackText.text = newText;
        feedbackPanelCanvasGroup.alpha = 1f;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / feedbackSlideDuration;
            feedbackRect.anchoredPosition = Vector2.Lerp(feedbackOffScreenPos, feedbackOnScreenPos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        // stay visible for 5 seconds before auto-slide-out
        yield return new WaitForSeconds(5f);
        FeedbackSlideOut();
    }

    // --- Feedback panel slide out ---
    private IEnumerator AnimateFeedbackOut()
    {
        if (feedbackRect == null)
            yield break;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / feedbackSlideDuration;
            feedbackRect.anchoredPosition = Vector2.Lerp(feedbackOnScreenPos, feedbackOffScreenPos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        feedbackPanelCanvasGroup.alpha = 0;
        feedbackText.text = "";
        isFeedbackVisible = false;
    }

    // --- Flicker Startup Sequence ---
    private IEnumerator StartupFlicker()
    {
        // Visor slides down first
        if (alphavisionRoot != null)
        {
            Vector2 startPos = visorStartPos;
            Vector2 endPos = visorEndPos;
            alphavisionRoot.anchoredPosition = startPos;
            yield return new WaitForSeconds(visorSlideDelay);

            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / visorSlideDuration;
                float eased = Mathf.SmoothStep(0, 1, t);
                alphavisionRoot.anchoredPosition = Vector2.Lerp(startPos, endPos, eased);
                yield return null;
            }
        }

        // After visor slides in, flicker panels
        yield return StartCoroutine(FlickerPanel(scorePanelCanvasGroup));
        yield return StartCoroutine(FlickerPanel(confidencePanelCanvasGroup));
        yield return StartCoroutine(FlickerPanel(feedbackPanelCanvasGroup));
    }

    private IEnumerator FadeInDatePortrait()
    {
        if (datePortrait == null)
        {
            Debug.LogWarning("Date portrait not assigned!");
            yield break;
        }

        // Ensure it's visible and reset transparency
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

    private IEnumerator FlickerPanel(CanvasGroup panel)
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

    // --- Optional Beta Behavior Flash ---
    [YarnCommand("trigger_beta_warning")]
    public void TriggerBetaWarning()
    {
        if (betaFlashRoutine != null)
            StopCoroutine(betaFlashRoutine);
        betaFlashRoutine = StartCoroutine(BetaFlashRoutine());
    }

    private IEnumerator BetaFlashRoutine()
    {
        if (betaCanvasGroup == null)
            yield break;

        // make sure it's active and visible
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
