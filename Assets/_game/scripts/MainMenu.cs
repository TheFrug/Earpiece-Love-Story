using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string firstLevel;
    [SerializeField] private GameObject CreditsPanel;

    public void StartGame()
    {
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
