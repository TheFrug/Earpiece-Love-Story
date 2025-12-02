#nullable enable
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;

public class AlphavisionController : MonoBehaviour
{
    public static AlphavisionController? Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private RectTransform alphavisionRoot = default!;

    [Header("Panels")]
    [SerializeField] private CanvasGroup scorePanelCanvasGroup = default!;
    [SerializeField] private CanvasGroup confidencePanelCanvasGroup = default!;
    [SerializeField] private CanvasGroup feedbackPanelCanvasGroup = default!;

    [Header("Slide Settings")]
    [SerializeField] private Vector2 visorStartPos = new Vector2(0, 800);
    [SerializeField] private Vector2 visorEndPos = new Vector2(0, 0);
    [SerializeField] private float visorSlideDuration = 1.2f;
    [SerializeField] private float visorSlideDelay = 0.2f;

    [Header("Flicker Settings")]
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private int flashCount = 3;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Initialize panel states
        if (scorePanelCanvasGroup != null)
            scorePanelCanvasGroup.alpha = 0;
        if (confidencePanelCanvasGroup != null)
            confidencePanelCanvasGroup.alpha = 0;
        if (feedbackPanelCanvasGroup != null)
            feedbackPanelCanvasGroup.alpha = 0;
    }

    public void RunStartupFlicker()
    {
        StartCoroutine(StartupFlicker());
    }

    public void RunGlassesRemoval()
    {
        StartCoroutine(RemoveGlassesRoutine());
    }

    // --- Main Coroutine ---
    private IEnumerator StartupFlicker()
    {
        // Step 1: Slide in the visor
        if (alphavisionRoot != null)
        {
            alphavisionRoot.anchoredPosition = visorStartPos;
            yield return new WaitForSeconds(visorSlideDelay);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / visorSlideDuration;
                float eased = Mathf.SmoothStep(0f, 1f, t);
                alphavisionRoot.anchoredPosition = Vector2.Lerp(visorStartPos, visorEndPos, eased);
                yield return null;
            }

            alphavisionRoot.anchoredPosition = visorEndPos;
        }

        // Step 2: Ensure score panel starts invisible and flicker it in
        if (scorePanelCanvasGroup != null)
            scorePanelCanvasGroup.alpha = 0;
        yield return StartCoroutine(FlickerPanel(scorePanelCanvasGroup));

        // Step 3: Ensure feedback panel starts invisible and flicker it in
        if (feedbackPanelCanvasGroup != null)
            feedbackPanelCanvasGroup.alpha = 0;
        yield return StartCoroutine(FlickerPanel(feedbackPanelCanvasGroup));
    }

    // --- Flicker Logic ---
    private IEnumerator FlickerPanel(CanvasGroup? panel)
    {
        if (panel == null)
            yield break;

        for (int i = 0; i < flashCount; i++)
        {
            panel.alpha = 0.8f;
            yield return new WaitForSeconds(flashDuration);
            panel.alpha = 0;
            yield return new WaitForSeconds(flashDuration);
        }

        panel.alpha = 1;
    }

    // --- Utility Accessors ---
    public void SetPanelVisibility(bool visible)
    {
        if (scorePanelCanvasGroup != null)
            scorePanelCanvasGroup.alpha = visible ? 1 : 0;
        if (confidencePanelCanvasGroup != null)
            confidencePanelCanvasGroup.alpha = visible ? 1 : 0;
        if (feedbackPanelCanvasGroup != null)
            feedbackPanelCanvasGroup.alpha = visible ? 1 : 0;
    }

    private IEnumerator RemoveGlassesRoutine()
    {
        // Flicker panels OFF (reverse of turning them on)
        yield return StartCoroutine(FlickerPanelOff(scorePanelCanvasGroup));
        yield return StartCoroutine(FlickerPanelOff(confidencePanelCanvasGroup));
        yield return StartCoroutine(FlickerPanelOff(feedbackPanelCanvasGroup));

        // Slide visor upward, offscreen
        if (alphavisionRoot != null)
        {
            float t = 0f;
            Vector2 start = visorEndPos;
            Vector2 end = visorStartPos;

            while (t < 1f)
            {
                t += Time.deltaTime / visorSlideDuration;
                float eased = Mathf.SmoothStep(0, 1, t);
                alphavisionRoot.anchoredPosition = Vector2.Lerp(start, end, eased);
                yield return null;
            }

            alphavisionRoot.gameObject.SetActive(false);
        }
    }

    private IEnumerator FlickerPanelOff(CanvasGroup? panel)
    {
        if (panel == null)
            yield break;

        for (int i = 0; i < flashCount; i++)
        {
            panel.alpha = 0;
            yield return new WaitForSeconds(flashDuration);
            panel.alpha = 1;
            yield return new WaitForSeconds(flashDuration);
        }

        panel.alpha = 0;
    }
}
