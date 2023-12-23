using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class MainMenuScr : MonoBehaviour
{
    //public Transform menu;
    public Button PlayButton;
    public Button ChangeDeckButton;
    public Button SettingsButton;
    public Button ExitButton;
    public GameObject settingsPanel;

    public GameSettings Settings = new GameSettings();

    private void Awake()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "Settings.json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            Settings = JsonUtility.FromJson<GameSettings>(json);
        }
        else
        {
            Settings.soundVolume = .5f;
            Settings.timer = 120;
            Settings.timerIsOn = true;
            Settings.difficulty = "Normal";
        }
        AudioListener.volume = Settings.soundVolume;
    }

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
