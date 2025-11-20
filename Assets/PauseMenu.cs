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
    public Material decoderMaterial;          // your special shader material
    private Material originalTMPMaterial;     // cached original material instance

    private bool isPaused = false;

    void Start()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);

        // Bind buttons and toggle (make sure these references are set in the inspector)
        if (pauseButton != null) pauseButton.onClick.AddListener(TogglePauseMenu);
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        if (decoderToggle != null) decoderToggle.onValueChanged.AddListener(SetDecoderMode);

        // Capture TMP material one frame later to avoid TMP instantiating after Start
        if (decoderTMP != null) {
            StartCoroutine(CaptureOriginalTMPMaterialNextFrame());
        } else {
            Debug.LogWarning("[PauseMenu] decoderTMP is null in inspector.");
        }

        // Sanity: ensure we have an Image component reference
        if (decoderOverlayImage == null) {
            Debug.LogWarning("[PauseMenu] decoderOverlayImage is null in inspector.");
        }
    }

    private IEnumerator CaptureOriginalTMPMaterialNextFrame()
    {
        yield return null; // wait one frame
        originalTMPMaterial = decoderTMP.fontMaterial;
        if (originalTMPMaterial == null) {
            Debug.LogWarning("[PauseMenu] Could not capture original TMP material (null).");
        } else {
            Debug.Log("[PauseMenu] Captured original TMP material.");
        }
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

    /// <summary>
    /// Turn decoder mode on/off.
    /// This explicitly ENABLES or DISABLES the Image component (decoderOverlayImage.enabled),
    /// and swaps the TMP material (fontMaterial) accordingly.
    /// If the decoder overlay GameObject is inactive, we activate it so the Image component can be toggled.
    /// </summary>
    public void SetDecoderMode(bool on)
    {
        if (decoderOverlayImage == null) {
            Debug.LogError("[PauseMenu] SetDecoderMode called but decoderOverlayImage is null.");
            return;
        }

        // If the whole GameObject is inactive, enable it so the Image component can be toggled.
        // If you guarantee the GameObject will always be active, you can remove this line.
        if (!decoderOverlayImage.gameObject.activeSelf) {
            decoderOverlayImage.gameObject.SetActive(true);
            Debug.Log("[PauseMenu] Activated decoderOverlayImage GameObject so Image component can be toggled.");
        }

        // THIS is the exact line you asked for: enable/disable the Image component itself.
        decoderOverlayImage.enabled = on;
        decoderOverlayImage.raycastTarget = on; // optional: make it non-interactable when off

        // Swap TMP material. We set fontMaterial to the decoder when turning on,
        // and restore the cached original instance when turning off.
        if (decoderTMP != null) {
            // If originalTMPMaterial hasn't been captured yet, do nothing but warn.
            if (originalTMPMaterial == null && !on) {
                Debug.LogWarning("[PauseMenu] originalTMPMaterial not captured yet; cannot restore.");
            }

            if (on) {
                if (decoderMaterial != null) {
                    decoderTMP.fontMaterial = decoderMaterial;
                } else {
                    Debug.LogWarning("[PauseMenu] decoderMaterial is null in inspector.");
                }
            } else {
                if (originalTMPMaterial != null) {
                    decoderTMP.fontMaterial = originalTMPMaterial;
                } else {
                    Debug.LogWarning("[PauseMenu] originalTMPMaterial is null; cannot restore TMP material.");
                }
            }
        } else {
            Debug.LogWarning("[PauseMenu] decoderTMP is null; skipping material swap.");
        }

        Debug.Log($"[PauseMenu] Decoder mode set to {(on ? "ON" : "OFF")} — Image.enabled = {decoderOverlayImage.enabled}");

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            decoderOverlayImage.canvas.rootCanvas.GetComponent<RectTransform>()
        );
    }
}
