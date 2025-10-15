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
    public GameObject betaBehaviorPanel;

    [Header("Flash Settings")]
    public float flashDuration = 0.2f;
    public int flashCount = 3;
    public float holdTime = 1f;
    public float fadeTime = 1f;

    private CanvasGroup betaCanvasGroup;
    private Coroutine betaFlashRoutine;

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
        if (feedbackText != null)
            feedbackText.text = newText;
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

        // Ensure visibility starts at full opacity
        betaCanvasGroup.alpha = 1;

        // Flash a few times
        for (int i = 0; i < flashCount; i++)
        {
            betaCanvasGroup.alpha = 1;
            yield return new WaitForSeconds(flashDuration);
            betaCanvasGroup.alpha = 0;
            yield return new WaitForSeconds(flashDuration);
        }

        // Hold visible
        betaCanvasGroup.alpha = 1;
        yield return new WaitForSeconds(holdTime);

        // Fade out smoothly
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
}
