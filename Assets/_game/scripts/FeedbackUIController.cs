#nullable enable
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;

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
    [SerializeField] private float autoHideDelay = 5f;

    private RectTransform? feedbackRect;
    private Coroutine? feedbackRoutine;
    private bool isFeedbackVisible = false;

    private void Awake()
    {
        if (feedbackPanel != null)
            feedbackRect = feedbackPanel.GetComponent<RectTransform>();

        if (feedbackPanelCanvasGroup != null)
            feedbackPanelCanvasGroup.alpha = 0f;
    }

    public void SetFeedback(string newText)
    {
        if (feedbackRoutine != null)
            StopCoroutine(feedbackRoutine);

        // If already visible, just update text without reanimation
        if (isFeedbackVisible)
        {
            feedbackTextShaded.text = newText;
            feedbackTextNormal.text = newText;
        }
        else
        {
            feedbackRoutine = StartCoroutine(AnimateFeedbackIn(newText));
        }
    }

        public void FeedbackSlideOut()
    {
        if (feedbackRoutine != null)
            StopCoroutine(feedbackRoutine);

        feedbackRoutine = StartCoroutine(AnimateFeedbackOut());
    }

    // --- Animation Coroutines ---
    private IEnumerator AnimateFeedbackIn(string newText)
    {
        if (feedbackRect == null || feedbackTextShaded == null)
            yield break;

        feedbackTextShaded.text = newText;
        feedbackTextNormal.text = newText;
        feedbackPanelCanvasGroup.alpha = 1f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / feedbackSlideDuration;
            feedbackRect.anchoredPosition = Vector2.Lerp(
                feedbackOffScreenPos,
                feedbackOnScreenPos,
                Mathf.SmoothStep(0f, 1f, t)
            );
            isFeedbackVisible = true;
            yield return null;
        }

        // Stay visible for a bit, then slide out
        yield return new WaitForSeconds(autoHideDelay);
        //FeedbackSlideOut();
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
