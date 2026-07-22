using UnityEngine;

/// <summary>
/// BGM/SFX 재생 및 볼륨 채널 관리
/// 게임 시작 시 저장된 볼륨 설정을 자동 적용
/// 
/// Setup:
///   루트 레벨 빈 오브젝트에 부착 (DontDestroyOnLoad 자동 적용)
///   AudioSource 2개가 자동 생성됨 (BGM용, SFX용)
/// </summary>
public class AudioManager : SingletonMonoBehaviour<AudioManager>
{
    private AudioSource bgmSource;
    private AudioSource sfxSource;

    private float bgmVolume = 1f;
    private float sfxVolume = 1f;

    protected override bool Persist => true;

    protected override void OnSingletonAwake()
    {
        CreateAudioSources();
        ApplySavedSettings();
    }

    private void CreateAudioSources()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

    private void ApplySavedSettings()
    {
        SetMasterVolume(PlayerPrefs.GetFloat(SettingsKeys.MASTER_VOLUME, 1f));
        SetBGMVolume(PlayerPrefs.GetFloat(SettingsKeys.BGM_VOLUME, 1f));
        SetSFXVolume(PlayerPrefs.GetFloat(SettingsKeys.SFX_VOLUME, 1f));
    }

    // -- Volume Control --

    public void SetMasterVolume(float value)
    {
        AudioListener.volume = Mathf.Clamp01(value);
    }

    public void SetBGMVolume(float value)
    {
        bgmVolume = Mathf.Clamp01(value);
        if (bgmSource != null)
            bgmSource.volume = bgmVolume;
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }

    public float GetBGMVolume() => bgmVolume;
    public float GetSFXVolume() => sfxVolume;

    // -- Playback --

    /// <summary>
    /// BGM 재생 (기존 BGM은 교체됨)
    /// </summary>
    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null || clip == null)
            return;

        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null)
            bgmSource.Stop();
    }

    /// <summary>
    /// 효과음 1회 재생 (겹쳐 재생 가능)
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null)
            return;

        sfxSource.PlayOneShot(clip, sfxVolume);
    }
}
