using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class GameSettings
{
    public float soundVolume;
    public int timer;
    public bool timerIsOn;
    public string difficulty; // Easy, Normal, Hard
}

public class SettingsManager : MonoBehaviour
{

    public GameSettings currentSettings = new GameSettings();
    public Slider soundSlider;
    public TextMeshProUGUI soundTxt;
    public ToggleGroup timerToggleGroup;
    public ToggleGroup difficultyToggleGroup;

    public GameObject pausePanel, settingsPanel;

    public AudioSource audioSource;


    private void Awake()
    {
        LoadSettings();
        if (soundSlider != null)
            soundSlider.onValueChanged.AddListener(OnSoundVolumeChanged);

        AddToggleListeners(timerToggleGroup, OnTimerToggleChanged);
        AddToggleListeners(difficultyToggleGroup, OnDifficultyToggleChanged);
    }

    private void AddToggleListeners(ToggleGroup toggleGroup, UnityAction<bool> callback)
    {
        foreach (Toggle toggle in toggleGroup.GetComponentsInChildren<Toggle>())
        {
            toggle.onValueChanged.AddListener(callback);
        }
    }

    public void LoadSettings()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "Settings.json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            currentSettings = JsonUtility.FromJson<GameSettings>(json);
        }
        else
        {
            CreateDefaultSettings();
        }

        ApplySettingsToUI();
    }

    public void SaveSettings()
    {
        string json = JsonUtility.ToJson(currentSettings, true);
        string filePath = Path.Combine(Application.persistentDataPath, "Settings.json");
        File.WriteAllText(filePath, json);
    }

    void CreateDefaultSettings()
    {
        TextAsset settingsAsset = Resources.Load<TextAsset>("Settings/Settings");
        if (settingsAsset != null)
        {
            currentSettings = JsonUtility.FromJson<GameSettings>(settingsAsset.text);
        }
        else
        {
            currentSettings.soundVolume = .5f;
            currentSettings.timer = 120;
            currentSettings.timerIsOn = true;
            currentSettings.difficulty = "Normal";
        }
        SaveSettings();
    }

    private void ApplySettingsToUI()
    {
        if (soundSlider != null)
        {
            soundSlider.value = currentSettings.soundVolume;
            soundTxt.text = (currentSettings.soundVolume * 100).ToString("F0");
        }


        foreach (Transform toggleTransform in timerToggleGroup.transform)
        {
            Toggle toggle = toggleTransform.GetComponent<Toggle>();
            if (toggle != null)
            {
                toggle.isOn = false;
            }
        }

        Toggle toggleToActivate = null;

        switch (currentSettings.timer)
        {
            case 0:
                toggleToActivate = timerToggleGroup.transform.Find("OffToggle").GetComponent<Toggle>();
                break;
            case 60:
                toggleToActivate = timerToggleGroup.transform.Find("60sToggle").GetComponent<Toggle>();
                break;
            case 120:
                toggleToActivate = timerToggleGroup.transform.Find("120sToggle").GetComponent<Toggle>();
                break;
            case 180:
                toggleToActivate = timerToggleGroup.transform.Find("180sToggle").GetComponent<Toggle>();
                break;

        }
        if (toggleToActivate != null)
        {
            toggleToActivate.isOn = true;
        }

        if (difficultyToggleGroup != null)
        {
            foreach (Transform toggleTransform in difficultyToggleGroup.transform)
            {
                Toggle toggle = toggleTransform.GetComponent<Toggle>();
                if (toggle != null)
                {
                    toggle.isOn = false;
                }
            }

            toggleToActivate = null;

            switch (currentSettings.difficulty)
            {
                case "Easy":
                    toggleToActivate = difficultyToggleGroup.transform.Find("EasyToggle").GetComponent<Toggle>();
                    break;
                case "Normal":
                    toggleToActivate = difficultyToggleGroup.transform.Find("NormalToggle").GetComponent<Toggle>();
                    break;
                case "Hard":
                    toggleToActivate = difficultyToggleGroup.transform.Find("HardToggle").GetComponent<Toggle>();
                    break;

            }

            if (toggleToActivate != null)
            {
                toggleToActivate.isOn = true;
            }
        }
    }

    public void OnSoundVolumeChanged(float volume)
    {
        currentSettings.soundVolume = volume;
        AudioListener.volume = volume;
        soundTxt.text = (currentSettings.soundVolume * 100).ToString("F0");
    }

    public void OnTimerToggleChanged(bool firstentry)
    {
        Toggle activeToggle = timerToggleGroup.ActiveToggles().FirstOrDefault();

        if (activeToggle != null)
        {
            // Обновляем настройку таймера в зависимости от того, какой тоггл активен
            if (activeToggle.name == "OffToggle")
            {
                currentSettings.timer = 0;
                currentSettings.timerIsOn = false;
            }
            else if (activeToggle.name == "60sToggle")
            {
                currentSettings.timer = 60;
                currentSettings.timerIsOn = true;
            }
            else if (activeToggle.name == "120sToggle")
            {
                currentSettings.timer = 120;
                currentSettings.timerIsOn = true;
            }
            else if (activeToggle.name == "180sToggle")
            {
                currentSettings.timer = 180;
                currentSettings.timerIsOn = true;
            }
        }
    }

    public void OnDifficultyToggleChanged(bool firstentry)
    {
        Toggle activeToggle = difficultyToggleGroup.ActiveToggles().FirstOrDefault();

        if (activeToggle != null)
        {
            // Обновляем настройку таймера в зависимости от того, какой тоггл активен
            if (activeToggle.name == "EasyToggle")
            {
                currentSettings.difficulty = "Easy";
            }
            else if (activeToggle.name == "NormalToggle")
            {
                currentSettings.difficulty = "Normal";
            }
            else if (activeToggle.name == "HardToggle")
            {
                currentSettings.difficulty = "Hard";
            }
        }
    }

    public void BackToPause()
    {
        SaveSettings();
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);

    }

    public void BackToMenu()
    {
        SaveSettings();
        settingsPanel.SetActive(false);
    }

}
