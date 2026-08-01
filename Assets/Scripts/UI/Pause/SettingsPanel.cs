using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 설정 패널 컨트롤러 (사운드, 해상도, 전체화면)
/// 부팅 시 설정 적용은 AudioManager / DisplaySettingsApplier가 담당하고,
/// 이 패널은 사용자 변경을 받아 적용 + 저장만 한다.
/// 
/// 해상도 변경 시 공용 ConfirmDialog로 "유지할까요?"를 묻고,
/// 15초 내 확인이 없으면 이전 해상도로 자동 복원한다.
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

    [Tooltip("해상도 확인 팝업 자동 복원 시간(초)")]
    [SerializeField] private float resolutionConfirmTimeout = 15f;

    [Header("Controls")]
    [SerializeField] private Button backButton;

    private PauseMenuUI pauseMenuUI;
    private readonly List<Resolution> availableResolutions = new List<Resolution>();

    // 해상도 변경 확인 대기 중 복원용 스냅샷
    private int previousResolutionIndex;
    private int previousWidth;
    private int previousHeight;

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
            options.Add($"{res.width} x {res.height}");

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
        {
            int index = FindCurrentResolutionIndex();
            resolutionDropdown.SetValueWithoutNotify(index);
            previousResolutionIndex = index;
            CacheCurrentResolution();
        }

        if (fullscreenToggle != null)
            fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
    }

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

    private void CacheCurrentResolution()
    {
        previousWidth = Screen.width;
        previousHeight = Screen.height;
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

    /// <summary>
    /// 해상도 변경 시 즉시 적용한 뒤 확인 팝업을 띄운다
    /// 확인하면 유지, 취소/타임아웃이면 이전 해상도로 복원
    /// </summary>
    private void OnResolutionChanged(int index)
    {
        if (index < 0 || index >= availableResolutions.Count)
            return;

        var res = availableResolutions[index];
        bool fullscreen = fullscreenToggle != null ? fullscreenToggle.isOn : Screen.fullScreen;

        // 새 해상도 즉시 적용
        Screen.SetResolution(res.width, res.height, fullscreen);

        // 확인 팝업 (콜백 방식)
        if (ConfirmDialog.Instance != null)
        {
            ConfirmDialog.Instance.Show(
                $"{res.width} x {res.height}\n\n이 해상도를 유지할까요?",
                onConfirm: () => KeepResolution(index, res.width, res.height),
                onCancel: RevertResolution,
                autoTimeoutSeconds: resolutionConfirmTimeout
            );
        }
        else
        {
            // ConfirmDialog 없으면 확인 없이 바로 확정
            KeepResolution(index, res.width, res.height);
        }
    }

    private void KeepResolution(int index, int width, int height)
    {
        PlayerPrefs.SetInt(SettingsKeys.RESOLUTION_WIDTH, width);
        PlayerPrefs.SetInt(SettingsKeys.RESOLUTION_HEIGHT, height);
        PlayerPrefs.Save();

        previousResolutionIndex = index;
        CacheCurrentResolution();
    }

    private void RevertResolution()
    {
        bool fullscreen = fullscreenToggle != null ? fullscreenToggle.isOn : Screen.fullScreen;
        Screen.SetResolution(previousWidth, previousHeight, fullscreen);

        if (resolutionDropdown != null)
            resolutionDropdown.SetValueWithoutNotify(previousResolutionIndex);
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
