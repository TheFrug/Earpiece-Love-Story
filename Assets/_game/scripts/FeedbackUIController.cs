#nullable enable
using System.Collections;
using UnityEngine;
using TMPro;

public class FeedbackUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text feedbackTextShaded = null!;
    [SerializeField] private TMP_Text feedbackTextNormal = null!;
    [SerializeField] private GameObject feedbackPanel = null!;
    [SerializeField] private CanvasGroup feedbackPanelCanvasGroup = null!;

    [Header("Animation Settings")]
    [SerializeField] private float feedbackSlideDuration = 0.6f;
    [SerializeField] private Vector2 feedbackOnScreenPos = new Vector2(0, 0);
    [SerializeField] private Vector2 feedbackOffScreenPos = new Vector2(0, -250);

    [Header("Manual/Startup Settings")]
    [SerializeField] private bool manualMode = false; // if true, AnimateFeedbackIn doesn't auto-show
    private bool feedbackLoadedIn = false; // tracks if panel has been initially loaded

    private RectTransform? feedbackRect;
    private Coroutine? feedbackRoutine;
    private bool isFeedbackVisible = false;

    private void Awake()
    {
        if (feedbackPanel != null)
            feedbackRect = feedbackPanel.GetComponent<RectTransform>();

        // Only hide at start if it hasn't been loaded in yet
        if (!feedbackLoadedIn)
            feedbackPanelCanvasGroup.alpha = 0f;
    }

    public void SetFeedback(string newText)
    {
        if (feedbackRoutine != null)
            StopCoroutine(feedbackRoutine);

        // Mark as loaded in immediately
        if (!feedbackLoadedIn)
        {
            feedbackLoadedIn = true;
            // Make sure panel is visible immediately
            if (feedbackPanelCanvasGroup != null)
                feedbackPanelCanvasGroup.alpha = 1f;
        }

        // Always update text
        feedbackTextShaded.text = newText;
        feedbackTextNormal.text = newText;

        // Only animate in if it hasn't been animated yet
        if (!isFeedbackVisible && !manualMode)
        {
            feedbackRoutine = StartCoroutine(AnimateFeedbackIn());
        }
    }

    // Animation coroutine now does not touch alpha
    private IEnumerator AnimateFeedbackIn()
    {
        if (feedbackRect == null)
            yield break;

        float t = 0f;
        Vector2 startPos = feedbackRect.anchoredPosition;
        while (t < 1f)
        {
            t += Time.deltaTime / feedbackSlideDuration;
            feedbackRect.anchoredPosition = Vector2.Lerp(
                startPos,
                feedbackOnScreenPos,
                Mathf.SmoothStep(0f, 1f, t)
            );
            isFeedbackVisible = true;
            yield return null;
        }

        feedbackRect.anchoredPosition = feedbackOnScreenPos;
        isFeedbackVisible = true;
    }

    public void FeedbackSlideOut()
    {
        if (feedbackRoutine != null)
            StopCoroutine(feedbackRoutine);

        feedbackRoutine = StartCoroutine(AnimateFeedbackOut());
    }

    private IEnumerator AnimateFeedbackOut()
    {
        if (feedbackRect == null)
            yield break;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / feedbackSlideDuration;
            feedbackRect.anchoredPosition = Vector2.Lerp(
                feedbackOnScreenPos,
                feedbackOffScreenPos,
                Mathf.SmoothStep(0f, 1f, t)
            );
            yield return null;
        }

        feedbackPanelCanvasGroup.alpha = 0f;
        feedbackTextShaded.text = "";
        feedbackTextNormal.text = "";
        isFeedbackVisible = false;
    }
}
