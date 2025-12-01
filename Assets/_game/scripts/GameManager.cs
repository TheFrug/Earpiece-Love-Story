#nullable enable
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager? Instance { get; private set; }

    // --- Core Game Variables ---
    [Header("Core Stats")]
    public int confidence = 0;
    public int dateScore = 0;
    public bool betaBehaviorDetected = false;

    [Header("Date Portrait Animation")]
    [SerializeField] private Image? datePortrait;
    [SerializeField] private float dateFadeDuration = 1.5f; // seconds

    [Header("Controllers and Managers")]
    [SerializeField] private AlphavisionController alphavision = null!;
    [SerializeField] private BetaWarningController betaWarning = null!;
    [SerializeField] private FeedbackUIController feedback = null!;
    [SerializeField] private StatManager stats = null!;
    [SerializeField] private ExpressionController expression = null!;
    [SerializeField] private ItemPresenter item = null!;

    [Header("FX & End Game")]
    [SerializeField] private GameObject? endPanel;
    [SerializeField] private FadeToBlackEffect? fadeEffect;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "MainScene")
            return;

        RebindAllReferences();
    }

    private void RebindAllReferences()
    {
        // Find all controllers as before...
        alphavision = FindObjectOfType<AlphavisionController>(true);
        betaWarning = FindObjectOfType<BetaWarningController>(true);
        feedback = FindObjectOfType<FeedbackUIController>(true);
        stats = FindObjectOfType<StatManager>(true);
        expression = FindObjectOfType<ExpressionController>(true);
        item = FindObjectOfType<ItemPresenter>(true);
        datePortrait = GameObject.Find("img_Riley")?.GetComponent<Image>();

        // ---- NEW ----
        fadeEffect = FindObjectOfType<FadeToBlackEffect>(true);
        endPanel = GameObject.Find("p_EndMenuPanel");

        // Hide end panel initially
        if (endPanel != null)
            endPanel.SetActive(false);

        Debug.Log("[GameManager] References rebound:");
        Debug.Log($"  FadeEffect: {fadeEffect}");
        Debug.Log($"  EndMenuPanel: {endPanel}");
    }

    private void Update()
    {
        bool allowDebug =
#if UNITY_EDITOR
        true;       // Always allow in Editor
#else
        false;      // Disabled in builds unless debugMode toggled
#endif

        if (Input.GetKeyDown(KeyCode.R))
            ReloadScene();

        if (!allowDebug)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetFeedback("Remember: you are in charge here, not her.");

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SlideFeedbackOut();

        if (Input.GetKeyDown(KeyCode.Alpha3))
            TriggerBetaWarning();

        if (Input.GetKeyDown(KeyCode.Alpha4))
            RunStartupFlicker();

        if (Input.GetKeyDown(KeyCode.Alpha5))
            IncreaseScore(10, "Dominance");

        if (Input.GetKeyDown(KeyCode.Alpha6))
            DecreaseScore(10, "Never Show Weakness");

        if (Input.GetKeyDown(KeyCode.Alpha7))
            ShowItem("steak");

        if (Input.GetKeyDown(KeyCode.Alpha8))
            ShowItem("smartGlasses");

        if (Input.GetKeyDown(KeyCode.Alpha9))
            FadeToBlack();

        if (Input.GetKeyDown(KeyCode.Z))
            ChangeExpression("Happy");

        if (Input.GetKeyDown(KeyCode.X))
            ChangeExpression("Angry");

        if (Input.GetKeyDown(KeyCode.C))
            ChangeExpression("Confused");

        if (Input.GetKeyDown(KeyCode.V))
            ChangeExpression("Laughing");

        if (Input.GetKeyDown(KeyCode.B))
            ChangeExpression("Neutral");
    }

    private void ReloadScene()
    {
        StartCoroutine(ReloadAfterDelay(0.5f));
    }

    private IEnumerator ReloadAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        var runner = FindObjectOfType<DialogueRunner>();
        if (runner != null)
            runner.Stop();

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // --- Yarn Commands ---
    [YarnCommand("dateEnters")]
    public void DateEnters()
    {
        StartCoroutine(FadeInDatePortrait());
    }

    [YarnCommand("dateExits")]
    public void DateExits()
    {
        StartCoroutine(FadeOutDatePortrait());
    }

    [YarnCommand("fadeToBlack")]
    public void FadeToBlack()
    {
        if (fadeEffect == null)
            return;

        // Register the helper to fade in the end panel after fade out
        fadeEffect.SetOnFadeOutComplete(FadeEndPanelIn);

        fadeEffect.FadeOut();
    }

    [YarnCommand("runStartupFlicker")]
    public void RunStartupFlicker()
    {
        alphavision.RunStartupFlicker();
    }

    [YarnCommand("throwAwayGlasses")]
    public void ThrowAwayGlasses()
    {
        if (alphavision == null)
        {
            Debug.LogWarning("GameManager: No Alphavision reference!");
            return;
        }

        alphavision.RunGlassesRemoval();
    }

    [YarnCommand("triggerBetaWarning")]
    public void TriggerBetaWarning()
    {
        betaWarning.TriggerBetaWarning();
    }

    [YarnCommand("setFeedback")]
    public void SetFeedback(string text)
    {
        feedback.SetFeedback(text);
    }

    [YarnCommand("feedbackSlideOut")]
    public void SlideFeedbackOut()
    {
        feedback.FeedbackSlideOut();
    }

    [YarnCommand("increaseScore")]
    public void IncreaseScore(int amount, string reason)
    {
        stats.IncreaseScore(amount, reason);
    }

    [YarnCommand("decreaseScore")]
    public void DecreaseScore(int amount, string reason)
    {
        stats.DecreaseScore(amount, reason);
    }

    [YarnCommand("changeExpression")]
    public void ChangeExpression(string expressionKey)
    {
        if (expression == null)
        {
            Debug.LogWarning("GameManager: ExpressionController reference not set!");
            return;
        }

        expression.ChangeExpression(expressionKey);
    }

    [YarnCommand("showItem")]
    public void ShowItem(string itemCode)
    {
        item.ShowItem(itemCode);
    }

    // --- Coroutines ---
    private IEnumerator FadeInDatePortrait()
    {
        if (datePortrait == null)
        {
            Debug.LogWarning("Date portrait not assigned!");
            yield break;
        }

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

    private IEnumerator FadeOutDatePortrait()
    {
        if (datePortrait == null)
        {
            Debug.LogWarning("Date portrait not assigned!");
            yield break;
        }

        Color c = datePortrait.color;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / dateFadeDuration;
            c.a = Mathf.SmoothStep(1f, 0f, t);
            datePortrait.color = c;
            yield return null;
        }

        c.a = 0f;
        datePortrait.color = c;
        datePortrait.gameObject.SetActive(false);
    }

    // --- NEW: Fade End Panel Helper ---
    private void FadeEndPanelIn()
    {
        if (endPanel == null)
            return;

        endPanel.SetActive(true);

        CanvasGroup cg = endPanel.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = endPanel.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
        }

        StartCoroutine(FadeCanvasGroup(cg, 0f, 1f, 1f)); // fade in over 1 second
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            cg.alpha = Mathf.Lerp(start, end, t);
            yield return null;
        }
        cg.alpha = end;
    }
}
