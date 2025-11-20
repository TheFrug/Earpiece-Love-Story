#nullable enable
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;
using UnityEngine.SceneManagement;

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

    [Header("Fade to Black")]
    [SerializeField] private Image? fadeOverlay;
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Controllers and Managers")]
    [SerializeField] private AlphavisionController alphavision = null!;
    [SerializeField] private BetaWarningController betaWarning = null!;
    [SerializeField] private FeedbackUIController feedback = null!;
    [SerializeField] private StatManager stats = null!;
    [SerializeField] private ExpressionController expression = null!;
    [SerializeField] private ItemPresenter item = null!;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            ReloadScene();

        if (Input.GetKeyDown(KeyCode.Escape))
            QuitGame();

        // Debug Commands
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
            ShowItemCommand("steak");
        
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

    [YarnCommand("fadeToBlack")]
    public void FadeToBlack()
    {
        StartCoroutine(FadeScreenToBlack());
    }

    [YarnCommand("runStartupFlicker")]
    public void RunStartupFlicker()
    {
        alphavision.RunStartupFlicker();
    }

    [YarnCommand("triggerBetaWarning")]
    public void TriggerBetaWarning()
    {
        betaWarning.TriggerBetaWarning();
    }

    [YarnCommand("setFeedback")]
    public void SetFeedback(string text)
    {
        feedback.SetFeedbackText(text);
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
    public void ChangeExpressionFromYarn(string expressionKey)
    {
        if (expression == null) {
            Debug.LogWarning("GameManager: ExpressionController reference not set!");
            return;
        }

        expression.ChangeExpression(expressionKey);
    }

    [YarnCommand("showItem")]
    public void ShowItemCommand(string itemCode)
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

    private IEnumerator FadeScreenToBlack()
    {
        if (fadeOverlay == null)
        {
            Debug.LogWarning("Fade overlay not assigned!");
            yield break;
        }

        fadeOverlay.gameObject.SetActive(true);
        Color c = fadeOverlay.color;
        c.a = 0f;
        fadeOverlay.color = c;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            c.a = Mathf.SmoothStep(0f, 1f, t);
            fadeOverlay.color = c;
            yield return null;
        }

        c.a = 1f;
        fadeOverlay.color = c;
    }
}
