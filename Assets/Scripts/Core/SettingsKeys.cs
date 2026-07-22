/// <summary>
/// 설정 저장에 사용하는 PlayerPrefs 키 모음
/// SettingsPanel, AudioManager, DisplaySettingsApplier가 공유
/// </summary>
public static class SettingsKeys
{
    public const string MASTER_VOLUME = "MasterVolume";
    public const string BGM_VOLUME = "BGMVolume";
    public const string SFX_VOLUME = "SFXVolume";

    public const string RESOLUTION_WIDTH = "ResolutionWidth";
    public const string RESOLUTION_HEIGHT = "ResolutionHeight";
    public const string FULLSCREEN = "Fullscreen";
}
