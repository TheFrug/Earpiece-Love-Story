using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("References")]
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

    void Start()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);

        // Bind buttons and toggle (make sure these references are set in the inspector)
        if (pauseButton != null) pauseButton.onClick.AddListener(TogglePauseMenu);
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        if (decoderToggle != null) decoderToggle.onValueChanged.AddListener(SetDecoderMode);

        // Sanity: ensure we have an Image component reference
        if (decoderOverlayImage == null) {
            Debug.LogWarning("[PauseMenu] decoderOverlayImage is null in inspector.");
        }

        SetDecoderMode(false);
    }

    public void TogglePauseMenu()
    {
        if (isPaused) {
            ResumeGame();
        } 
        else {
            PauseGame();
        }
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
        SceneManager.LoadScene("MainMenu");
    }

    public void SetDecoderMode(bool on)
    {
        if (decoderOverlayImage == null) {
            Debug.LogError("[PauseMenu] SetDecoderMode called but decoderOverlayImage is null.");
            return;
        }

        if (normalTMP == null || decoderTMP == null) {
            Debug.LogError("[PauseMenu] One or both TMP references are missing.");
            return;
        }

        // Always ensure the overlay object is active so its Image component can toggle
        if (!decoderOverlayImage.gameObject.activeSelf) {
            decoderOverlayImage.gameObject.SetActive(true);
        }

        // Overlay image visibility
        decoderOverlayImage.enabled = on;
        decoderOverlayImage.raycastTarget = on;

        // Switch between text objects
        decoderTMP.gameObject.SetActive(on);
        normalTMP.gameObject.SetActive(!on);

        Debug.Log($"[PauseMenu] DecoderMode {(on ? "ENABLED" : "DISABLED")}");
    }
}
