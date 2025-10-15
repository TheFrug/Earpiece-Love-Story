using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;

/// <summary>
/// Manages global game state, Alphavision UI, and Yarn-accessible methods.
/// </summary>
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
    public float feedbackVisibleTime = 5f;
    public Vector2 feedbackOnScreenPos = new Vector2(0, 0);
    public Vector2 feedbackOffScreenPos = new Vector2(0, -250);

    private CanvasGroup betaCanvasGroup;
    private Coroutine betaFlashRoutine;
    private Coroutine feedbackRoutine;
    private RectTransform feedbackRect;

    [Header("Alphavision Slide-In")]
    public RectTransform alphavisionRoot;  // The parent RectTransform of the entire visor UI
    public Vector2 visorStartPos = new Vector2(0, 800);  // start position off-screen (adjust as needed)
    public Vector2 visorEndPos = new Vector2(0, 0);      // final centered position
    public float visorSlideDuration = 1.2f;
    public float visorSlideDelay = 0.2f;


    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Cache references
        if (betaBehaviorPanel != null)
            betaCanvasGroup = betaBehaviorPanel.GetComponent<CanvasGroup>();
        if (feedbackPanel != null)
            feedbackRect = feedbackPanel.GetComponent<RectTransform>();

        // Initialize panel visibility
        if (scorePanelCanvasGroup != null) scorePanelCanvasGroup.alpha = 0;
        if (confidencePanelCanvasGroup != null) confidencePanelCanvasGroup.alpha = 0;
        if (feedbackPanelCanvasGroup != null) feedbackPanelCanvasGroup.alpha = 0;
    }

    private void Start()
    {
        // Startup boot flicker
        StartCoroutine(StartupFlicker());
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
            dateScoreText.text = $"Date Score: {dateScore} pts";
    }

    [YarnCommand("set_feedback")]
    public void SetFeedbackText(string newText)
    {
        if (feedbackRoutine != null)
            StopCoroutine(feedbackRoutine);
        feedbackRoutine = StartCoroutine(AnimateFeedback(newText));
    }

    // --- Beta Behavior Flashing Warning ---
    [YarnCommand("trigger_beta_warning")]
    public void TriggerBetaBehavior()
    {
        if (betaFlashRoutine != null)
            StopCoroutine(betaFlashRoutine);
        betaFlashRoutine = StartCoroutine(FlashBetaWarning());
    }

    private IEnumerator FlashBetaWarning()
    {
        betaBehaviorDetected = true;
        if (betaBehaviorPanel == null)
        {
            Debug.LogWarning("Beta Behavior Panel not assigned!");
            yield break;
        }

        betaBehaviorPanel.SetActive(true);
        if (betaCanvasGroup == null)
            betaCanvasGroup = betaBehaviorPanel.GetComponent<CanvasGroup>();

        betaCanvasGroup.alpha = 1;

        // Flashing
        for (int i = 0; i < flashCount; i++)
        {
            betaCanvasGroup.alpha = 1;
            yield return new WaitForSeconds(flashDuration);
            betaCanvasGroup.alpha = 0;
            yield return new WaitForSeconds(flashDuration);
        }

        // Hold and fade
        betaCanvasGroup.alpha = 1;
        yield return new WaitForSeconds(holdTime);

        float t = 0;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            betaCanvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeTime);
            yield return null;
        }

        betaBehaviorPanel.SetActive(false);
        betaBehaviorDetected = false;
    }

    // --- Alphavision Startup Flicker ---
    // --- Alphavision Startup Sequence ---
    private IEnumerator StartupFlicker()
    {
        // Safety check
        if (alphavisionRoot != null)
        {
            // --- STEP 1: Simulate glasses being put on ---
            alphavisionRoot.anchoredPosition = visorStartPos;
            yield return new WaitForSeconds(visorSlideDelay);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / visorSlideDuration;
                float eased = Mathf.SmoothStep(0, 1, t);
                alphavisionRoot.anchoredPosition = Vector2.Lerp(visorStartPos, visorEndPos, eased);
                yield return null;
            }

            alphavisionRoot.anchoredPosition = visorEndPos;
        }

        // --- STEP 2: Begin flickering the individual panels ---
        yield return new WaitForSeconds(0.3f);
        yield return FlickerPanel(scorePanelCanvasGroup);
        yield return FlickerPanel(confidencePanelCanvasGroup);
        yield return FlickerPanel(feedbackPanelCanvasGroup);
    }


    private IEnumerator FlickerPanel(CanvasGroup cg)
    {
        if (cg == null) yield break;

        for (int i = 0; i < 6; i++)
        {
            cg.alpha = (i % 2 == 0) ? 1 : 0;
            yield return new WaitForSeconds(0.1f);
        }

        // Smooth final fade-in
        float t = 0;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0, 1, t / 0.3f);
            yield return null;
        }

        cg.alpha = 1;
    }

    // --- Feedback Panel Animation ---
    private IEnumerator AnimateFeedback(string message)
    {
        if (feedbackText == null || feedbackRect == null)
            yield break;

        feedbackText.text = message;

        // Slide on
        yield return SlidePanel(feedbackOffScreenPos, feedbackOnScreenPos, feedbackSlideDuration);

        yield return new WaitForSeconds(feedbackVisibleTime);

        // Slide off
        yield return SlidePanel(feedbackOnScreenPos, feedbackOffScreenPos, feedbackSlideDuration);

        feedbackText.text = "";
    }

    private IEnumerator SlidePanel(Vector2 from, Vector2 to, float duration)
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float easedT = Mathf.SmoothStep(0, 1, t / duration);
            feedbackRect.anchoredPosition = Vector2.Lerp(from, to, easedT);
            yield return null;
        }
        feedbackRect.anchoredPosition = to;
    }
}
