using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("References (optional - inspector or auto-find)")]
    public GameObject pauseMenuPanel;
    public Button pauseButton;                // the little corner button
    public Button resumeButton;
    public Button mainMenuButton;
    public Toggle decoderToggle;

    [Header("Decoder Mode")]
    public Image decoderOverlayImage;         // the visual overlay (Image component)
    public TMP_Text decoderTMP;               // text getting the shader
    public TMP_Text normalTMP;

    private bool isPaused = false;

    private void Awake()
    {
        // Subscribe to sceneLoaded so we rebind whenever new scene loads
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            PauseGame();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Small delay ensures that the UI hierarchy is instantiated.
        // StartCoroutine allows us to wait a frame and then bind.
        StartCoroutine(RebindNextFrame());
    }

    private IEnumerator RebindNextFrame()
    {
        yield return null; // wait one frame so objects are available
        TryRebindAll();
    }

    /// <summary>
    /// Tries to find references and bind listeners. Safe to call repeatedly.
    /// </summary>
    private void TryRebindAll()
    {
        // If pause panel wasn't assigned, try find it first (we need it active to find children reliably)
        if (pauseMenuPanel == null)
            pauseMenuPanel = GameObject.Find("p_PauseMenuPanel");

        // If we found a panel, temporarily ensure it's active so child objects exist to find
        bool weActivatedPanel = false;
        if (pauseMenuPanel != null && !pauseMenuPanel.activeSelf)
        {
            pauseMenuPanel.SetActive(true);
            weActivatedPanel = true;
        }

        // Find other objects by name if not assigned in inspector
        if (pauseButton == null)
            pauseButton = GameObject.Find("b_PauseButton")?.GetComponent<Button>();

        if (resumeButton == null)
            resumeButton = GameObject.Find("b_Resume")?.GetComponent<Button>();

        if (mainMenuButton == null)
            mainMenuButton = GameObject.Find("b_MainMenu")?.GetComponent<Button>();

        if (decoderToggle == null)
            decoderToggle = GameObject.Find("Toggle")?.GetComponent<Toggle>();

        if (decoderOverlayImage == null)
            decoderOverlayImage = GameObject.Find("img_DecoderFilter")?.GetComponent<Image>();

        if (decoderTMP == null)
            decoderTMP = GameObject.Find("txt_FeedbackShaded")?.GetComponent<TMP_Text>();

        if (normalTMP == null)
            normalTMP = GameObject.Find("txt_FeedbackNormal")?.GetComponent<TMP_Text>();

        // Bind UI (remove previous listeners first to avoid duplicates)
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(TogglePauseMenu);
        }
        else Debug.LogWarning("[PauseMenu] pauseButton not found.");

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(ResumeGame);
        }
        else Debug.LogWarning("[PauseMenu] resumeButton not found.");

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
        else Debug.LogWarning("[PauseMenu] mainMenuButton not found.");

        if (decoderToggle != null)
        {
            decoderToggle.onValueChanged.RemoveAllListeners();
            decoderToggle.onValueChanged.AddListener(SetDecoderMode);
        }
        else Debug.LogWarning("[PauseMenu] decoderToggle not found.");

        // Ensure overlay and text objects exist
        if (decoderOverlayImage == null) Debug.LogWarning("[PauseMenu] decoderOverlayImage not found.");
        if (decoderTMP == null) Debug.LogWarning("[PauseMenu] decoderTMP not found.");
        if (normalTMP == null) Debug.LogWarning("[PauseMenu] normalTMP not found.");

        // Initialize defaults
        SetDecoderMode(true);

        // Hide panel again if we temporarily showed it
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    // Public toggle used by button
    public void TogglePauseMenu()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        print("Returning to Main Menu");
        SceneManager.LoadScene("MainMenu");
    }

    public void SetDecoderMode(bool on)
    {
        if (decoderOverlayImage == null)
        {
            Debug.LogWarning("[PauseMenu] SetDecoderMode called but decoderOverlayImage is null.");
            return;
        }

        if (decoderTMP == null || normalTMP == null)
        {
            Debug.LogWarning("[PauseMenu] SetDecoderMode called but text refs are null.");
            return;
        }

        // Ensure overlay object exists and toggle it
        decoderOverlayImage.gameObject.SetActive(true);
        decoderOverlayImage.enabled = on;
        decoderOverlayImage.raycastTarget = on;

        decoderTMP.gameObject.SetActive(on);
        normalTMP.gameObject.SetActive(!on);

        Debug.Log($"[PauseMenu] DecoderMode {(on ? "ENABLED" : "DISABLED")}");
    }
}
