using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScr : MonoBehaviour
{
    //public Transform menu;
    public Button PlayButton;
    public Button ChangeDeckButton;
    public Button SettingsButton;
    public Button ExitButton;
    public GameObject settingsPanel;

    void Start()
    {
        PlayButton.onClick.AddListener(OnPlayButtonClicked);
        ChangeDeckButton.onClick.AddListener(OnChangeDeckButtonClicked);
        SettingsButton.onClick.AddListener(OnSettingsButtonClicked);
        ExitButton.onClick.AddListener(OnExitButtonClicked);
    }

    public void OnPlayButtonClicked()
    {
        SceneManager.LoadScene("Gameplay");
    }

    public void OnChangeDeckButtonClicked()
    {
        SceneManager.LoadScene("ChangeDeck_Scene");
    }

    public void OnSettingsButtonClicked()
    {
        settingsPanel.SetActive(true);
    }

    public void OnExitButtonClicked()
    {
        Application.Quit();
    }

}
