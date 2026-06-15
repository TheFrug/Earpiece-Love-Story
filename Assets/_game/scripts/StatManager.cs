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
    // Updated to transition from Blue to Gold
    [SerializeField] private Color scoreColor0to59 = new Color(0.1f, 0.3f, 0.8f);   // Deep Blue
    [SerializeField] private Color scoreColor60to69 = new Color(0.2f, 0.6f, 0.9f);   // Sky Blue
    [SerializeField] private Color scoreColor70to79 = new Color(0.4f, 0.8f, 0.7f);   // Teal / Aqua
    [SerializeField] private Color scoreColor80to89 = new Color(0.9f, 0.9f, 0.4f);   // Pale Gold
    [SerializeField] private Color scoreColor90to100 = new Color(1f, 0.84f, 0f);     // Solid Gold

    private Vector3 scoreChangeTextStartPos;
    private Coroutine scoreChangeRoutine;

    private void Start()
    {
        scoreChangeTextStartPos = scoreChangeText.rectTransform.anchoredPosition;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (confidenceText != null)
            confidenceText.text = $"{confidence}";

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
            confidenceText.text = $"{confidence}";
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

    public void IncreaseScore(int amount, string reason)
    {
        if (scoreChangeRoutine != null)
            StopCoroutine(scoreChangeRoutine);
        scoreChangeRoutine = StartCoroutine(ScoreChangeAnimation(amount, reason, true));
    }

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

        // Always use the Solid Gold color for readability
        scoreChangeText.color = scoreColor90to100;
        scoreChangeText.alpha = 1f;

        // Floating animation
        Vector3 endPos = scoreChangeTextStartPos + new Vector3(0, 80f, 0);
        float duration = 2.5f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float eased = Mathf.SmoothStep(0f, 1f, t);
            scoreChangeText.rectTransform.anchoredPosition = Vector3.Lerp(scoreChangeTextStartPos, endPos, eased);
            scoreChangeText.alpha = Mathf.Lerp(1f, 0f, Mathf.Pow(t, 1.5f));
            yield return null;
        }

        scoreChangeText.text = string.Empty;
        scoreChangeText.rectTransform.anchoredPosition = scoreChangeTextStartPos;
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