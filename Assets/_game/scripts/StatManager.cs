using System.Collections;
using UnityEngine;
using TMPro;
using Yarn.Unity;

/// <summary>
/// Handles confidence and date score values, their display, and
/// the floating score-change text animation.
/// Attach to the ScorePanel GameObject.
/// </summary>
public class StatManager : MonoBehaviour
{
    [Header("Stat Values")]
    [Range(0, 100)] public int confidence = 0;
    [Range(0, 100)] public int dateScore = 0;

    [Header("UI References")]
    [SerializeField] private TMP_Text confidenceText;
    [SerializeField] private TMP_Text dateScoreText;
    [SerializeField] private TMP_Text scoreChangeText;

    [Header("Score Colors (Low → High)")]
    [SerializeField] private Color scoreColor0to59 = new Color(0.8f, 0.2f, 0.2f);
    [SerializeField] private Color scoreColor60to69 = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color scoreColor70to79 = new Color(1f, 0.8f, 0f);
    [SerializeField] private Color scoreColor80to89 = new Color(0.85f, 1f, 0.4f);
    [SerializeField] private Color scoreColor90to100 = new Color(0.02f, 0.87f, 0.45f);

    private Coroutine scoreChangeRoutine;

    private void Start()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (confidenceText != null)
            confidenceText.text = $"Confidence: {confidence}%";

        if (dateScoreText != null)
        {
            dateScoreText.text = $"{dateScore}";
            dateScoreText.color = GetScoreColor(dateScore);
        }
    }

    // ---------------------------------------------------------------------
    // Public API -----------------------------------------------------------
    // ---------------------------------------------------------------------

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

    // ---------------------------------------------------------------------
    // Yarn Commands (3.0 syntax – safe for current runtime)
    // ---------------------------------------------------------------------

    [YarnCommand("increase_score")]
    public void IncreaseScore(int amount, string reason)
    {
        if (scoreChangeRoutine != null)
            StopCoroutine(scoreChangeRoutine);
        scoreChangeRoutine = StartCoroutine(ScoreChangeAnimation(amount, reason, true));
    }

    [YarnCommand("decrease_score")]
    public void DecreaseScore(int amount, string reason)
    {
        if (scoreChangeRoutine != null)
            StopCoroutine(scoreChangeRoutine);
        scoreChangeRoutine = StartCoroutine(ScoreChangeAnimation(amount, reason, false));
    }

    // ---------------------------------------------------------------------
    // Internal logic -------------------------------------------------------
    // ---------------------------------------------------------------------

    private IEnumerator ScoreChangeAnimation(int amount, string reason, bool isIncrease)
    {
        int newScore = isIncrease ? dateScore + amount : dateScore - amount;
        SetDateScore(newScore);

        if (scoreChangeText == null)
            yield break;

        // Configure text
        scoreChangeText.text = (isIncrease ? "+" : "-") + amount + "  " + reason;
        scoreChangeText.color = isIncrease ? scoreColor90to100 : scoreColor0to59;
        scoreChangeText.alpha = 1f;

        // Floating animation
        Vector3 startPos = scoreChangeText.rectTransform.anchoredPosition;
        Vector3 endPos = startPos + new Vector3(0, 80f, 0);
        float duration = 2.5f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float eased = Mathf.SmoothStep(0f, 1f, t);
            scoreChangeText.rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, eased);
            scoreChangeText.alpha = Mathf.Lerp(1f, 0f, Mathf.Pow(t, 1.5f));
            yield return null;
        }

        scoreChangeText.text = string.Empty;
        scoreChangeText.rectTransform.anchoredPosition = startPos;
    }

    private Color GetScoreColor(int score)
    {
        if (score < 60) return scoreColor0to59;
        if (score < 70) return scoreColor60to69;
        if (score < 80) return scoreColor70to79;
        if (score < 90) return scoreColor80to89;
        return scoreColor90to100;
    }
}
