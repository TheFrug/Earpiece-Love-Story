using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string firstLevel;
    [SerializeField] private GameObject CreditsPanel;
    [SerializeField] private string mainSceneAmbienceKey = "LoopRestaurant";

    public void StartGame()
    {
        print("Starting");
        // Pass the string key instead of the direct AudioClip reference
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.TransitionAmbience(mainSceneAmbienceKey, 1.5f);
        }
        if (FadeToBlackEffect.Instance == null)
        {
            Debug.LogError("FadeToBlackEffect.Instance is NULL!");
            return;
        }

        FadeToBlackEffect.Instance.FadeToScene(firstLevel);
    }

    public void OpenOptions()
    {

    }

    public void CloseOptions()
    {

    }

    public void OpenCredits()
    {
        if (CreditsPanel != null) CreditsPanel.SetActive(true);
    }

    public void CloseCredits()
    {
        if (CreditsPanel != null) CreditsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Working");
    }
}
