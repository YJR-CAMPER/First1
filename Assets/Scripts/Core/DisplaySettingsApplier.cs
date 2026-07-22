using UnityEngine;

/// <summary>
/// 게임 시작 시 저장된 해상도/전체화면 설정을 적용
/// SettingsPanel은 비활성 상태로 시작하므로 부팅 시 적용은 이 컴포넌트가 담당
/// 
/// Setup:
///   루트 레벨 매니저 오브젝트 아무 곳에나 부착 (AudioManager와 같은 오브젝트 가능)
/// </summary>
public class DisplaySettingsApplier : MonoBehaviour
{
    private void Awake()
    {
        ApplySavedDisplaySettings();
    }

    private void ApplySavedDisplaySettings()
    {
        // 저장된 값이 없으면 현재 해상도 유지
        if (!PlayerPrefs.HasKey(SettingsKeys.RESOLUTION_WIDTH))
            return;

        int width = PlayerPrefs.GetInt(SettingsKeys.RESOLUTION_WIDTH, Screen.currentResolution.width);
        int height = PlayerPrefs.GetInt(SettingsKeys.RESOLUTION_HEIGHT, Screen.currentResolution.height);
        bool fullscreen = PlayerPrefs.GetInt(SettingsKeys.FULLSCREEN, 1) == 1;

        // 유효하지 않은 값 방어
        if (width < 640 || height < 480)
            return;

        Screen.SetResolution(width, height, fullscreen);
    }
}
