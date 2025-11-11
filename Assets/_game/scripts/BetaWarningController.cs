#nullable enable
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class BetaWarningController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject betaBehaviorPanel = null!;
    private CanvasGroup? betaCanvasGroup;

    [Header("Flash Settings")]
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private int flashCount = 3;
    [SerializeField] private float holdTime = 1f;
    [SerializeField] private float fadeTime = 1f;

    private Coroutine? betaFlashRoutine;

    private void Awake()
    {
        if (betaBehaviorPanel != null)
            betaCanvasGroup = betaBehaviorPanel.GetComponent<CanvasGroup>();
    }

    public void TriggerBetaWarning()
    {
        if (betaFlashRoutine != null)
            StopCoroutine(betaFlashRoutine);

        betaFlashRoutine = StartCoroutine(BetaFlashRoutine());
    }

    private IEnumerator BetaFlashRoutine()
    {
        if (betaBehaviorPanel == null || betaCanvasGroup == null)
        {
            Debug.LogWarning("Beta warning panel or CanvasGroup not assigned.");
            yield break;
        }

        betaBehaviorPanel.SetActive(true);
        betaCanvasGroup.alpha = 0f;

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
