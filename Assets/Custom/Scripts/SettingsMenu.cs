using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class SettingsMenu : Menu
{
    [OdinSerialize] Button resetButton;
    [OdinSerialize] Toggle fullscreenToggle;
    [OdinSerialize] Toggle subtitlesToggle;
    [OdinSerialize] Dictionary<string, Slider> volumeSliders = new Dictionary<string, Slider>();
    SettingsData settingsData = new SettingsData();
    [OdinSerialize] AudioMixer audioMixer;
    bool initialized = false;
    [OdinSerialize] List<GameObject> panelList = new List<GameObject>();
    int currentPanelIdx = 0;
    public void Initialize()
    {
        ConnectSettingsUI();
        LoadSettings();
        SyncSettingValues();
        UpdateSettings();

        initialized = true;
    }

    public void Start()
    { 
        if (!initialized) Initialize();
    }

    void ConnectSettingsUI()
    {
        List<Object> objects;
        objects = new List<Object>(Resources.FindObjectsOfTypeAll(typeof(Toggle)));
        foreach (var obj in objects)
        {
            Toggle toggle = (Toggle)obj;
            toggle.onValueChanged.AddListener(delegate { ActivateResetButton(); });
            toggle.onValueChanged.AddListener((var) => { UpdateSettings(); SaveSettings(); });
        }

        objects = new List<Object>(Resources.FindObjectsOfTypeAll(typeof(Slider)));
        foreach (var obj in objects)
        {
            Slider slider = (Slider)obj;
            slider.onValueChanged.AddListener(delegate { ActivateResetButton(); });
            slider.onValueChanged.AddListener((var) => { UpdateSettings(); SaveSettings(); });
        }
    }

    public void ActivateResetButton()
    {
        resetButton.interactable = true;
    }

    public void ResetSettings()
    {
        settingsData.isFullscreen = true;
        settingsData.subtitlesOn = true;

        settingsData.masterVolume = 50f;
        settingsData.dialogueVolume = 50f;
        settingsData.musicVolume = 50f;
        settingsData.sfxVolume = 50f;

        SyncSettingValues();
    }

    public void OnBack(InputValue value)
    {
        UIManager.Instance.CloseMenu();
        ResetPanels();
    }

    public void OnNext(InputValue value)
    {
        GoToNextPanel();
    }

    // public void OnMenu(InputValue value)
    // {
    //     LoadMainMenu();
    // }

    // public async void LoadMainMenu()
    // {
    //     ScenesManager.Instance.ShowLoadingScreen();
    //     await ScenesManager.Instance.LoadScene("MainMenu");
    //     ScenesManager.Instance.HideLoadingScreen();
    //     ScenesManager.Instance.ShowScene();
    // }

    public void SaveSettings()
    {
        string saveJson = JsonUtility.ToJson(settingsData);
        PlayerPrefs.SetString("settingsData", saveJson);
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey("settingsData"))
        {
            string loadJson = PlayerPrefs.GetString("settingsData");
            settingsData = JsonUtility.FromJson<SettingsData>(loadJson);
        }
        else // If no save
        {
            ResetSettings();
        }
    }

    public void SyncSettingValues()
    {
        fullscreenToggle.isOn = settingsData.isFullscreen;
        subtitlesToggle.isOn = settingsData.subtitlesOn;

        volumeSliders["master"].value = settingsData.masterVolume;
        volumeSliders["dialogue"].value = settingsData.dialogueVolume;
        volumeSliders["music"].value = settingsData.musicVolume;
        volumeSliders["sfx"].value = settingsData.sfxVolume;
    }

    public void UpdateSettings()
    {
        settingsData.isFullscreen = fullscreenToggle.isOn;
        settingsData.subtitlesOn = subtitlesToggle.isOn;

        settingsData.masterVolume = volumeSliders["master"].value;
        settingsData.dialogueVolume = volumeSliders["dialogue"].value;
        settingsData.musicVolume = volumeSliders["music"].value;
        settingsData.sfxVolume = volumeSliders["sfx"].value;

        Screen.fullScreen = settingsData.isFullscreen;
        UIManager.Instance.subtitlesOn = settingsData.subtitlesOn;

        // print(settingsData.masterVolume);
        // print(Mathf.Log10(Mathf.Clamp(settingsData.masterVolume, 0.0001f, 100f) / 100f) * 20f);

        audioMixer.SetFloat("Master", Mathf.Log10(Mathf.Clamp(settingsData.masterVolume, 0.0001f, 100f) / 100f) * 20f);
        audioMixer.SetFloat("Dialogue", Mathf.Log10(Mathf.Clamp(settingsData.dialogueVolume, 0.0001f, 100f) / 100f) * 20f);
        audioMixer.SetFloat("Music", Mathf.Log10(Mathf.Clamp(settingsData.musicVolume, 0.0001f, 100f) / 100f) * 20f);
        audioMixer.SetFloat("SFX", Mathf.Log10(Mathf.Clamp(settingsData.sfxVolume, 0.0001f, 100f) / 100f) * 20f);
        audioMixer.SetFloat("Ambience", Mathf.Log10(Mathf.Clamp(settingsData.sfxVolume, 0.0001f, 100f) / 100f) * 20f);
    }

    void ResetPanels()
    {
        foreach (GameObject panel in panelList)
        {
            panel.SetActive(false);
        }
        panelList[0].SetActive(true);
        currentPanelIdx = 0;
    }

    void GoToNextPanel()
    {
        panelList[currentPanelIdx].SetActive(false);
        
        currentPanelIdx++;
        if (currentPanelIdx == panelList.Count) currentPanelIdx = 0;

        panelList[currentPanelIdx].SetActive(true);
    }
}

public struct SettingsData
{
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
    public float dialogueVolume;
    public bool isFullscreen;
    public bool subtitlesOn;
}
