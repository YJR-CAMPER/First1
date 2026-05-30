using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 설정 패널 컨트롤러 (사운드, 조작)
/// </summary>
public class SettingsPanel : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    
    [Header("Controls")]
    [SerializeField] private Button backButton;
    
    private PauseMenuUI pauseMenuUI;
    
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    private void Awake()
    {
        pauseMenuUI = GetComponentInParent<PauseMenuUI>();
        
        if (pauseMenuUI == null)
            pauseMenuUI = FindObjectOfType<PauseMenuUI>();
        
        SetupUI();
        LoadSettings();
    }

    private void SetupUI()
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

    private void LoadSettings()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        
        if (bgmVolumeSlider != null)
            bgmVolumeSlider.value = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1f);
        
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
    }

    private void OnMasterVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, value);
        ApplyMasterVolume(value);
    }

    private void OnBGMVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, value);
        ApplyBGMVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, value);
        ApplySFXVolume(value);
    }

    private void ApplyMasterVolume(float value)
    {
        AudioListener.volume = value;
    }

    private void ApplyBGMVolume(float value)
    {
        // AudioManager가 있으면 연동
        // AudioManager.Instance?.SetBGMVolume(value);
    }

    private void ApplySFXVolume(float value)
    {
        // AudioManager가 있으면 연동
        // AudioManager.Instance?.SetSFXVolume(value);
    }

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
    
    public static float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
    }
    
    public static float GetBGMVolume()
    {
        return PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1f);
    }
    
    public static float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
    }
}
