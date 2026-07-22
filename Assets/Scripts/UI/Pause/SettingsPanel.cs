using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 설정 패널 컨트롤러 (사운드, 해상도, 전체화면)
/// 부팅 시 설정 적용은 AudioManager / DisplaySettingsApplier가 담당하고,
/// 이 패널은 사용자 변경을 받아 적용 + 저장만 한다
/// </summary>
public class SettingsPanel : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Display Settings")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Controls")]
    [SerializeField] private Button backButton;

    private PauseMenuUI pauseMenuUI;
    private readonly List<Resolution> availableResolutions = new List<Resolution>();

    private void Awake()
    {
        pauseMenuUI = GetComponentInParent<PauseMenuUI>();

        if (pauseMenuUI == null)
            pauseMenuUI = FindObjectOfType<PauseMenuUI>();

        SetupAudioUI();
        SetupDisplayUI();
        LoadSettings();
    }

    // -- Setup --

    private void SetupAudioUI()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

        if (bgmVolumeSlider != null)
            bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
    }

    private void SetupDisplayUI()
    {
        if (resolutionDropdown != null)
        {
            PopulateResolutionDropdown();
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
    }

    /// <summary>
    /// 시스템이 지원하는 해상도 목록으로 드롭다운을 채움
    /// 주사율만 다른 중복 해상도는 하나로 합침
    /// </summary>
    private void PopulateResolutionDropdown()
    {
        availableResolutions.Clear();
        var seen = new HashSet<string>();

        foreach (var res in Screen.resolutions)
        {
            string key = $"{res.width}x{res.height}";
            if (seen.Contains(key))
                continue;

            seen.Add(key);
            availableResolutions.Add(res);
        }

        var options = new List<string>();
        foreach (var res in availableResolutions)
        {
            options.Add($"{res.width} x {res.height}");
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
    }

    // -- Load --

    private void LoadSettings()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = PlayerPrefs.GetFloat(SettingsKeys.MASTER_VOLUME, 1f);

        if (bgmVolumeSlider != null)
            bgmVolumeSlider.value = PlayerPrefs.GetFloat(SettingsKeys.BGM_VOLUME, 1f);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = PlayerPrefs.GetFloat(SettingsKeys.SFX_VOLUME, 1f);

        if (resolutionDropdown != null)
            resolutionDropdown.SetValueWithoutNotify(FindCurrentResolutionIndex());

        if (fullscreenToggle != null)
            fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
    }

    /// <summary>
    /// 현재 화면 해상도와 일치하는 드롭다운 인덱스를 찾음
    /// </summary>
    private int FindCurrentResolutionIndex()
    {
        int savedWidth = PlayerPrefs.GetInt(SettingsKeys.RESOLUTION_WIDTH, Screen.width);
        int savedHeight = PlayerPrefs.GetInt(SettingsKeys.RESOLUTION_HEIGHT, Screen.height);

        for (int i = 0; i < availableResolutions.Count; i++)
        {
            if (availableResolutions[i].width == savedWidth &&
                availableResolutions[i].height == savedHeight)
            {
                return i;
            }
        }

        return availableResolutions.Count > 0 ? availableResolutions.Count - 1 : 0;
    }

    // -- Audio Handlers --

    private void OnMasterVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(SettingsKeys.MASTER_VOLUME, value);

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMasterVolume(value);
        else
            AudioListener.volume = value;
    }

    private void OnBGMVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(SettingsKeys.BGM_VOLUME, value);

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetBGMVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(SettingsKeys.SFX_VOLUME, value);

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
    }

    // -- Display Handlers --

    private void OnResolutionChanged(int index)
    {
        if (index < 0 || index >= availableResolutions.Count)
            return;

        var res = availableResolutions[index];
        bool fullscreen = fullscreenToggle != null ? fullscreenToggle.isOn : Screen.fullScreen;

        Screen.SetResolution(res.width, res.height, fullscreen);

        PlayerPrefs.SetInt(SettingsKeys.RESOLUTION_WIDTH, res.width);
        PlayerPrefs.SetInt(SettingsKeys.RESOLUTION_HEIGHT, res.height);
    }

    private void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(SettingsKeys.FULLSCREEN, isFullscreen ? 1 : 0);
    }

    // -- Navigation --

    private void OnBackClicked()
    {
        PlayerPrefs.Save();

        if (pauseMenuUI != null)
            pauseMenuUI.CloseSettings();
    }

    private void OnDisable()
    {
        PlayerPrefs.Save();
    }
}
