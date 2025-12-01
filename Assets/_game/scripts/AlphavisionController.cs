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

        yield return StartCoroutine(FlickerPanel(scorePanelCanvasGroup));
        yield return StartCoroutine(FlickerPanel(confidencePanelCanvasGroup));
    }

    // --- Flicker Logic ---
    private IEnumerator FlickerPanel(CanvasGroup? panel)
    {
        if (panel == null)
            yield break;

        for (int i = 0; i < flashCount; i++)
        {
            panel.alpha = 1;
            yield return new WaitForSeconds(flashDuration);
            panel.alpha = 0;
            yield return new WaitForSeconds(flashDuration);
        }

        panel.alpha = 1;
    }

    // --- Utility Accessors (for other controllers) ---

    public void SetPanelVisibility(bool visible)
    {
        if (scorePanelCanvasGroup != null)
            scorePanelCanvasGroup.alpha = visible ? 1 : 0;
        if (confidencePanelCanvasGroup != null)
            confidencePanelCanvasGroup.alpha = visible ? 1 : 0;
    }

    private IEnumerator RemoveGlassesRoutine()
    {
        // Flicker panels OFF (reverse of turning them on)
        yield return StartCoroutine(FlickerPanelOff(scorePanelCanvasGroup));
        yield return StartCoroutine(FlickerPanelOff(confidencePanelCanvasGroup));

        // Slide visor upward, offscreen (reverse of StartupFlicker)
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

            // Fully hide once offscreen
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
