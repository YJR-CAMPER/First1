using System;
using UnityEngine;

/// <summary>
/// 게임 일시정지 상태를 관리하는 매니저
/// </summary>
public class PauseManager : SingletonMonoBehaviour<PauseManager>
{
    public event EventHandler<PauseStateChangedEventArgs> OnPauseStateChanged;

    public bool IsPaused { get; private set; }

    private float previousTimeScale = 1f;

    public void Pause()
    {
        if (IsPaused)
            return;

        IsPaused = true;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        OnPauseStateChanged?.Invoke(this, new PauseStateChangedEventArgs(true));
    }

    public void Resume()
    {
        if (!IsPaused)
            return;

        IsPaused = false;
        Time.timeScale = previousTimeScale;

        OnPauseStateChanged?.Invoke(this, new PauseStateChangedEventArgs(false));
    }

    public void TogglePause()
    {
        if (IsPaused)
            Resume();
        else
            Pause();
    }

    protected override void OnDestroy()
    {
        if (Instance == this)
        {
            Time.timeScale = 1f;
        }

        base.OnDestroy();
    }
}
